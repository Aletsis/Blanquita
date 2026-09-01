using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Exceptions;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Exceptions;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;

/// <summary>
/// Repositorio para acceder a documentos desde archivos FoxPro/DBF.
/// Implementa caché en memoria para reducir lecturas repetidas del archivo.
/// </summary>
public class FoxProDocumentRepository : IFoxProDocumentRepository
{
    private readonly IConfiguracionService _configService;
    private readonly IFoxProReaderFactory _readerFactory;
    private readonly ILogger<FoxProDocumentRepository> _logger;
    private readonly IMemoryCache _cache;

    // Configuración de caché
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string CacheKeyPrefix = "FoxProDocuments_";

    public FoxProDocumentRepository(
        IConfiguracionService configService,
        IFoxProReaderFactory readerFactory,
        ILogger<FoxProDocumentRepository> logger,
        IMemoryCache cache)
    {
        _configService = configService;
        _readerFactory = readerFactory;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<DocumentDto>> GetByDateAndBranchAsync(
        DateTime date, 
        int branchId, 
        CancellationToken cancellationToken = default)
    {
        // Crear clave de caché única por fecha
        var cacheKey = $"{CacheKeyPrefix}{date:yyyyMMdd}_{branchId}";

        // Intentar obtener del caché
        if (_cache.TryGetValue(cacheKey, out IEnumerable<DocumentDto>? cachedDocuments))
        {
            _logger.LogDebug(
                "Cache HIT: Documentos para fecha {Date} obtenidos del caché ({Count} documentos)",
                date.Date,
                cachedDocuments?.Count() ?? 0);
            
            return cachedDocuments ?? Enumerable.Empty<DocumentDto>();
        }

        _logger.LogDebug("Cache MISS: Leyendo documentos desde archivo DBF para fecha {Date}", date.Date);

        // Si no está en caché, leer del archivo
        var documents = await ReadDocumentsFromFileAsync(date, branchId, cancellationToken);

        // Guardar en caché con expiración
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            Size = documents.Count() // Ayuda a controlar el tamaño del caché
        };

        _cache.Set(cacheKey, documents, cacheOptions);

        _logger.LogInformation(
            "Documentos cacheados para fecha {Date} ({Count} documentos, expira en {Minutes} minutos)",
            date.Date,
            documents.Count(),
            CacheDuration.TotalMinutes);

        return documents;
    }

    private async Task<IEnumerable<DocumentDto>> ReadDocumentsFromFileAsync(
        DateTime date,
        int branchId,
        CancellationToken cancellationToken)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Mgw10008Path;
        var documents = new List<DocumentDto>();

        if (string.IsNullOrEmpty(filePath))
        {
            _logger.LogWarning("Ruta de archivo MGW10008 no configurada");
            return documents;
        }

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Archivo MGW10008 no encontrado: {FilePath}", filePath);
            throw new FoxProFileNotFoundException(filePath);
        }

        try
        {
            // Usar lector reverso ya que los documentos para una fecha específica (e.g. hoy o ayer) están al final del archivo
            using var reader = _readerFactory.CreateReverseReader(filePath);

            int consecutiveOlderCount = 0;
            const int maxConsecutiveOlderToStop = 30; // 30 registros seguidos más viejos nos indican que ya pasamos la fecha buscada

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var docDate = reader.GetDateTimeSafe("CFECHA");
                    var cancelado = reader.GetInt32Safe("CCANCELADO");

                    if (docDate.Date == date.Date)
                    {
                        consecutiveOlderCount = 0; // Reiniciar contador de viejos
                        
                        // Solo incluir facturas no canceladas (CCANCELADO = 0)
                        if (cancelado == 0)
                        {
                            documents.Add(FoxProDocumentMapper.MapToDto(reader));
                        }
                    }
                    else if (docDate.Date < date.Date && docDate != DateTime.MinValue)
                    {
                        consecutiveOlderCount++;
                        if (consecutiveOlderCount >= maxConsecutiveOlderToStop)
                        {
                            _logger.LogInformation("Deteniendo lectura reversa de MGW10008 al encontrar {Count} registros consecutivos anteriores a {Date}", consecutiveOlderCount, date.Date);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error al leer registro de documento, continuando...");
                    continue;
                }
            }

            _logger.LogInformation(
                "Se encontraron {Count} documentos para fecha {Date}", 
                documents.Count, 
                date.Date);

            // Al leer en reversa, invertimos el resultado para mantener el orden cronológico normal
            documents.Reverse();
            return documents;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Búsqueda de documentos cancelada");
            throw;
        }
        catch (Exception ex) when (ex is not FoxProFileNotFoundException)
        {
            _logger.LogError(ex, "Error al obtener documentos de FoxPro");
            throw new FoxProDataReadException("Error al leer documentos", filePath, ex);
        }
    }
    public async Task<IEnumerable<BillingReportItemDto>> GetBillingReportAsync(
        DateTime date, 
        string serie, 
        CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var invoicesPath = config.Mgw10045Path;
        var documentsPath = config.Mgw10008Path;

        if (string.IsNullOrEmpty(invoicesPath) || !File.Exists(invoicesPath))
        {
            _logger.LogWarning("Archivo MGW10045 no encontrado o no configurado: {FilePath}", invoicesPath);
            return Enumerable.Empty<BillingReportItemDto>();
        }

        if (string.IsNullOrEmpty(documentsPath) || !File.Exists(documentsPath))
        {
            _logger.LogWarning("Archivo MGW10008 no encontrado o no configurado: {FilePath}", documentsPath);
            throw new FoxProFileNotFoundException(documentsPath);
        }

        var billingItems = new Dictionary<string, BillingReportItemDto>();

        try
        {
            // Paso 1: Leer MGW10045 y filtrar por fecha y serie
            using (var reader = _readerFactory.CreateReader(invoicesPath))
            {
                // Validar columnas requeridas
                ValidateColumns(reader, "MGW10045", 
                    "CFECHAEMI", "CSERIE", "CFOLIO", "CRFC", "CRAZON", "CUUID", "CIDDOCTO", "CHORAEMI");

                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try 
                    {
                        var docDate = reader.GetDateTimeSafe("CFECHAEMI");
                        var docSerie = reader.GetStringSafe("CSERIE");

                        if (docDate.Date == date.Date && 
                            docSerie.Equals(serie, StringComparison.OrdinalIgnoreCase))
                        {
                            var item = FoxProBillingMapper.MapFromInvoice(reader);
                            if (!string.IsNullOrEmpty(item.IdDocumento))
                            {
                                billingItems[item.IdDocumento] = item;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al leer registro de factura (MGW10045). Posible error de datos.");
                    }
                }
            }

            _logger.LogInformation("Encontradas {Count} facturas en MGW10045 para fecha {Date} y serie {Serie}", 
                billingItems.Count, date.Date, serie);

            if (billingItems.Count == 0)
            {
                return Enumerable.Empty<BillingReportItemDto>();
            }

            // Paso 2: Leer MGW10008 y cruzar información
            int matchedCount = 0;
            using (var reader = _readerFactory.CreateReverseReader(documentsPath))
            {
                // Validar columnas requeridas
                ValidateColumns(reader, "MGW10008", 
                    "CIDDOCUM01", "CNETO", "CTOTAL", "CIMPUESTO1", "CCANCELADO", 
                    "CESTADO", "CENTREGADO", "CAUTUSBA01", "CFECHA", "CTEXTOEX03", "CIMPORTE03");

                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var idDocumento = reader.GetStringSafe("CIDDOCUM01"); // Using CIDDOCUM01 as generic ID/PK in 10008

                        if (billingItems.TryGetValue(idDocumento, out var item))
                        {
                            // Actualizar item con datos de MGW10008
                            billingItems[idDocumento] = FoxProBillingMapper.MapFromDocument(item, reader);
                            matchedCount++;

                            // Si ya cruzamos todas las facturas encontradas, podemos salir del bucle anticipadamente
                            if (matchedCount >= billingItems.Count)
                            {
                                _logger.LogInformation("Deteniendo lectura de MGW10008 para reporte de facturación al encontrar todas las coincidencias ({Count})", matchedCount);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                         _logger.LogWarning(ex, "Error al leer registro de documento (MGW10008). Posible error de datos.");
                    }
                }
            }
            
            return billingItems.Values.OrderBy(x => x.Folio).ToList();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Búsqueda de reporte de facturación cancelada");
            throw;
        }
        catch (InvalidOperationException) // Re-throw validation errors directly
        {
            throw;
        }
        catch (Exception ex) when (ex is not FoxProFileNotFoundException)
        {
            _logger.LogError(ex, "Error al generar reporte de facturación");
            throw new FoxProDataReadException("Error al leer archivos DBF para reporte", invoicesPath, ex);
        }
    }

    public async Task<IEnumerable<InvoiceDto>> GetInvoicesByClientIdAsync(int clientId, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var documentsPath = config.Mgw10008Path;
        var invoices = new List<InvoiceDto>();

        if (string.IsNullOrEmpty(documentsPath) || !File.Exists(documentsPath))
        {
             _logger.LogWarning("Archivo MGW10008 no encontrado o no configurado: {FilePath}", documentsPath);
             return invoices;
        }

        try
        {
            using var reader = _readerFactory.CreateReader(documentsPath);
            
             // Validar columnas requeridas
             // Assuming CSERIEDO01 and CFOLIO exist based on AdminPAQ schema, and CIDCLIEN01 is the link
            ValidateColumns(reader, "MGW10008", "CIDCLIEN01", "CSERIEDO01", "CFOLIO", "CFECHA");

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var idCliente = reader.GetInt32Safe("CIDCLIEN01");

                    if (idCliente == clientId)
                    {
                        var serie = reader.GetStringSafe("CSERIEDO01");
                        var folioDecimal = reader.GetDecimalSafe("CFOLIO");
                        var folio = (double)folioDecimal;
                        var fecha = reader.GetDateTimeSafe("CFECHA");
                        
                        // Construct filename: F + Serie + Folio (10 digits left padded with 0)
                        var folioStr = folioDecimal.ToString("0"); // Use decimal for string formatting to avoid scientific notation
                        folioStr = folioStr.PadLeft(10, '0');
                        
                        var fileName = $"F{serie}{folioStr}";

                        invoices.Add(new InvoiceDto
                        {
                            Serie = serie,
                            Folio = folio,
                            Fecha = fecha,
                            FileName = fileName,
                            ClientId = idCliente
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error al leer registro de factura para cliente {ClientId}", clientId);
                }
            }

            return invoices.OrderByDescending(x => x.Fecha).ToList();
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error al obtener facturas del cliente {ClientId}", clientId);
             throw;
        }
    }

    public async Task<IEnumerable<InvoiceDto>> GetRecentInvoicesAsync(DateTime sinceDate, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var documentsPath = config.Mgw10008Path;
        var invoices = new List<InvoiceDto>();

        if (string.IsNullOrEmpty(documentsPath) || !File.Exists(documentsPath))
        {
             _logger.LogWarning("Archivo MGW10008 no encontrado o no configurado: {FilePath}", documentsPath);
             return invoices;
        }

        try
        {
            // Usar lector reverso para eficiencia
            using var reader = _readerFactory.CreateReverseReader(documentsPath);
            
            ValidateColumns(reader, "MGW10008", "CIDCLIEN01", "CSERIEDO01", "CFOLIO", "CFECHA");

            int consecutiveOlderCount = 0;
            const int maxConsecutiveOlderToStop = 50; // Detenerse si vemos 50 registros seguidos anteriores a la fecha límite

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var fecha = reader.GetDateTimeSafe("CFECHA");

                    if (fecha < sinceDate)
                    {
                        consecutiveOlderCount++;
                        if (consecutiveOlderCount >= maxConsecutiveOlderToStop)
                        {
                            // Encontramos suficientes registros viejos de forma consecutiva, podemos asumir que ya terminamos de leer los recientes
                            _logger.LogInformation("Deteniendo lectura reversa de MGW10008 al encontrar {Count} registros consecutivos anteriores a {SinceDate}", consecutiveOlderCount, sinceDate);
                            break;
                        }
                        continue;
                    }

                    // Reiniciar contador si vemos un registro dentro del rango
                    consecutiveOlderCount = 0;

                    var idCliente = reader.GetInt32Safe("CIDCLIEN01");
                    var serie = reader.GetStringSafe("CSERIEDO01");
                    var folioDecimal = reader.GetDecimalSafe("CFOLIO");
                    var folio = (double)folioDecimal;
                    
                    var folioStr = folioDecimal.ToString("0");
                    folioStr = folioStr.PadLeft(10, '0');
                    var fileName = $"F{serie}{folioStr}";

                    invoices.Add(new InvoiceDto
                    {
                        Serie = serie,
                        Folio = folio,
                        Fecha = fecha,
                        FileName = fileName,
                        ClientId = idCliente
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error al leer registro en GetRecentInvoicesAsync");
                }
            }

            return invoices;
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error al obtener facturas recientes mediante lectura reversa");
             throw;
        }
    }

    public async Task<IEnumerable<ReturnReportItemDto>> GetReturnsReportAsync(
        int? year, 
        int? month, 
        string? serie, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        string? tipo = null, 
        CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var pos10008Path = config.Pos10008Path;
        var returns = new List<ReturnReportItemDto>();

        if (string.IsNullOrEmpty(pos10008Path) || !File.Exists(pos10008Path))
        {
             _logger.LogWarning("Archivo POS10008 no encontrado o no configurado: {FilePath}", pos10008Path);
             return returns;
        }

        try
        {
            // 1. Leer devoluciones directamente desde POS10008 (CIDDOCUM02 = 36)
            var returnDocIds = new HashSet<string>();
            var docToApertura = new Dictionary<string, int>();

            using (var reader = _readerFactory.CreateReader(pos10008Path))
            {
                ValidateColumns(reader, "POS10008", "CIDDOCUM01", "CIDDOCUM02", "CSERIEDO01", "CFOLIO", "CFECHA", "CNETO", "CIMPUESTO1", "CTOTAL", "CIDAPERTUR");

                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var docType = reader.GetInt32Safe("CIDDOCUM02");
                        if (docType != 36) continue; // 36 = Devolución de Venta POS

                        var docDate = reader.GetDateTimeSafe("CFECHA");
                        var docSerie = reader.GetStringSafe("CSERIEDO01")?.Trim() ?? string.Empty;

                        // Filtro de fecha
                        if (startDate.HasValue && endDate.HasValue)
                        {
                            if (docDate.Date < startDate.Value.Date || docDate.Date > endDate.Value.Date) continue;
                        }
                        else if (year.HasValue && month.HasValue)
                        {
                            if (docDate.Year != year.Value || docDate.Month != month.Value) continue;
                        }

                        // Filtro de serie
                        if (!string.IsNullOrWhiteSpace(serie) && !docSerie.Equals(serie.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var docId = reader.GetStringSafe("CIDDOCUM01")?.Trim() ?? string.Empty;
                        var folio = reader.GetDecimalSafe("CFOLIO");
                        var neto = reader.GetDecimalSafe("CNETO");
                        var impuesto = reader.GetDecimalSafe("CIMPUESTO1");
                        var total = reader.GetDecimalSafe("CTOTAL");
                        var cidApertur = reader.GetInt32Safe("CIDAPERTUR");

                        if (!string.IsNullOrEmpty(docId))
                        {
                            returnDocIds.Add(docId);
                            docToApertura[docId] = cidApertur;
                        }

                        returns.Add(new ReturnReportItemDto
                        {
                            IdDocumento = docId,
                            Serie = docSerie,
                            Folio = folio.ToString("0"),
                            Fecha = docDate,
                            Neto = neto,
                            Impuesto = impuesto,
                            Total = total,
                            Tipo = "Completa" // Por defecto, se recalculará al cruzar con partidas y venta original
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error al leer registro de devolución en POS10008");
                    }
                }
            }

            if (!returns.Any())
            {
                return returns;
            }

            // 2. Leer partidas de POS10010 para cada devolución encontrada
            var pos10010Path = config.Pos10010Path;
            var docDetails = new Dictionary<string, List<ReturnDetailDto>>();
            var productIds = new HashSet<string>();

            if (!string.IsNullOrEmpty(pos10010Path) && File.Exists(pos10010Path) && returnDocIds.Any())
            {
                try
                {
                    using var detailReader = _readerFactory.CreateReader(pos10010Path);
                    ValidateColumns(detailReader, "POS10010", "CIDDOCUM01", "CIDPRODU01", "CUNIDADES", "CPRECIO", "CNETO", "CIMPUESTO1", "CTOTAL");

                    while (detailReader.Read())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            var detailDocId = detailReader.GetStringSafe("CIDDOCUM01")?.Trim() ?? string.Empty;
                            if (!returnDocIds.Contains(detailDocId)) continue;

                            var prodId = detailReader.GetStringSafe("CIDPRODU01")?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(prodId))
                            {
                                productIds.Add(prodId);
                            }

                            var detailDto = new ReturnDetailDto
                            {
                                ProductId = prodId,
                                Units = (double)detailReader.GetDecimalSafe("CUNIDADES"),
                                Price = detailReader.GetDecimalSafe("CPRECIO"),
                                Net = detailReader.GetDecimalSafe("CNETO"),
                                Tax = detailReader.GetDecimalSafe("CIMPUESTO1"),
                                Total = detailReader.GetDecimalSafe("CTOTAL")
                            };

                            if (!docDetails.TryGetValue(detailDocId, out var dList))
                            {
                                dList = new List<ReturnDetailDto>();
                                docDetails[detailDocId] = dList;
                            }
                            dList.Add(detailDto);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Error al leer detalle en POS10010");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al leer POS10010 para detalle de devoluciones");
                }
            }

            // 3. Obtener nombres de productos desde MGW10005
            var productsFilePath = config.Mgw10005Path;
            var productNames = new Dictionary<string, string>();
            if (productIds.Any() && !string.IsNullOrEmpty(productsFilePath) && File.Exists(productsFilePath))
            {
                try
                {
                    using var productReader = _readerFactory.CreateReader(productsFilePath);
                    ValidateColumns(productReader, "MGW10005", "CIDPRODU01", "CNOMBREP01");

                    while (productReader.Read())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var pId = productReader.GetStringSafe("CIDPRODU01")?.Trim() ?? string.Empty;

                        if (productIds.Contains(pId))
                        {
                            var pName = productReader.GetStringSafe("CNOMBREP01");
                            productNames[pId] = pName;
                        }

                        if (productNames.Count == productIds.Count) break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error al leer nombres de productos en MGW10005");
                }
            }

            // 4. Cruzar con POS10042 para obtener referencia de ticket/venta original y calcular si es Completa o Parcial
            var pos10042Path = config.Pos10042Path;
            var aperturaToVentaRef = new Dictionary<int, (string Referencia, decimal? TotalVenta)>();

            if (!string.IsNullOrEmpty(pos10042Path) && File.Exists(pos10042Path))
            {
                try
                {
                    using var reader42 = _readerFactory.CreateReader(pos10042Path);
                    ValidateColumns(reader42, "POS10042", "CDEVOLUCIO", "CIDAPERTUR");

                    while (reader42.Read())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            var rawDevolucio = reader42.GetStringSafe("CDEVOLUCIO");
                            if (string.IsNullOrWhiteSpace(rawDevolucio)) continue;

                            var cidApertur = reader42.GetInt32Safe("CIDAPERTUR");
                            if (cidApertur <= 0) continue;

                            aperturaToVentaRef[cidApertur] = (rawDevolucio.Trim(), null);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Error al procesar registro en POS10042");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al leer POS10042 para cruce de devoluciones");
                }
            }

            // 5. Ensamblar DTO final con clasificación Completa / Parcial
            var result = new List<ReturnReportItemDto>();
            foreach (var item in returns)
            {
                var details = docDetails.TryGetValue(item.IdDocumento, out var dList) ? dList : new List<ReturnDetailDto>();
                foreach (var d in details)
                {
                    if (productNames.TryGetValue(d.ProductId, out var pName))
                    {
                        d.ProductName = pName;
                    }
                }

                string referencia = string.Empty;
                decimal? ventaOriginal = null;
                if (docToApertura.TryGetValue(item.IdDocumento, out var apId) && aperturaToVentaRef.TryGetValue(apId, out var vRef))
                {
                    referencia = vRef.Referencia;
                    ventaOriginal = vRef.TotalVenta;
                }

                // Determinación Completa vs Parcial:
                // Si la venta original existe y difiere del total devuelto, o si tiene 1 partida de N, se clasifica
                string tipoDevolucion = "Completa";
                if (ventaOriginal.HasValue && ventaOriginal.Value > 0)
                {
                    tipoDevolucion = Math.Abs(item.Total - ventaOriginal.Value) < 0.05m ? "Completa" : "Parcial";
                }
                else if (details.Count > 0 && details.Count < 2 && item.Total < 200m)
                {
                    // Heurística en devoluciones de mostrador con pocas piezas
                    tipoDevolucion = "Parcial";
                }

                var finalItem = item with
                {
                    Referencia = referencia,
                    Tipo = tipoDevolucion,
                    VentaOriginalTotal = ventaOriginal,
                    PartidasCount = details.Count,
                    Detalles = details
                };

                // Filtro por tipo si fue especificado
                if (!string.IsNullOrWhiteSpace(tipo) && !string.Equals(tipo, "Todas", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(finalItem.Tipo, tipo.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                result.Add(finalItem);
            }

            return result.OrderByDescending(x => x.Fecha).ThenBy(x => x.Folio).ToList();
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error al obtener reporte de devoluciones desde POS10008");
             throw;
        }
    }

    public async Task<IEnumerable<CancellationReportItemDto>> GetCancellationsReportAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? serie = null,
        string? tipo = null,
        CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var pos10008Path = config.Pos10008Path;
        var pos10010Path = config.Pos10010Path;
        var cancellations = new List<CancellationReportItemDto>();

        if (string.IsNullOrEmpty(pos10008Path) || !File.Exists(pos10008Path))
        {
            _logger.LogWarning("Archivo POS10008 no configurado o no existe: {FilePath}", pos10008Path);
            return cancellations;
        }

        try
        {
            // 1. Leer POS10008 buscando documentos cancelados (CCANCELADO = 1) y documentos activos para verificar partidas canceladas
            var completeCancelledDocs = new List<CancellationReportItemDto>();
            var activeDocDict = new Dictionary<string, (DateTime Fecha, string Serie, string Folio, string Cliente, int DocType, decimal Neto, decimal Impuesto, decimal Total)>();
            var allTargetDocIds = new HashSet<string>();

            using (var reader08 = _readerFactory.CreateReader(pos10008Path))
            {
                ValidateColumns(reader08, "POS10008", "CIDDOCUM01", "CIDDOCUM02", "CSERIEDO01", "CFOLIO", "CFECHA", "CNETO", "CIMPUESTO1", "CTOTAL", "CCANCELADO");

                while (reader08.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var docDate = reader08.GetDateTimeSafe("CFECHA");

                        // Filtro de fechas
                        if (startDate.HasValue && endDate.HasValue)
                        {
                            if (docDate.Date < startDate.Value.Date || docDate.Date > endDate.Value.Date) continue;
                        }

                        var docSerie = reader08.GetStringSafe("CSERIEDO01")?.Trim() ?? string.Empty;

                        // Filtro de serie
                        if (!string.IsNullOrWhiteSpace(serie) && !docSerie.Equals(serie.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var docId = reader08.GetStringSafe("CIDDOCUM01")?.Trim() ?? string.Empty;
                        if (string.IsNullOrEmpty(docId)) continue;

                        var docType = reader08.GetInt32Safe("CIDDOCUM02");
                        var cancelado = reader08.GetInt32Safe("CCANCELADO");
                        var folio = reader08.GetDecimalSafe("CFOLIO").ToString("0");
                        var neto = reader08.GetDecimalSafe("CNETO");
                        var impuesto = reader08.GetDecimalSafe("CIMPUESTO1");
                        var total = reader08.GetDecimalSafe("CTOTAL");

                        string docTypeName = docType switch
                        {
                            35 => "Venta POS",
                            36 => "Devolución POS",
                            17 => "Pedido POS",
                            _ => "Documento POS"
                        };

                        if (cancelado == 1)
                        {
                            // Cancelación Completa
                            completeCancelledDocs.Add(new CancellationReportItemDto
                            {
                                IdDocumento = docId,
                                Fecha = docDate,
                                Serie = docSerie,
                                Folio = folio,
                                TipoCancelacion = "Completa",
                                TipoDocumento = docTypeName,
                                Cliente = "PUBLICO GENERAL",
                                Neto = neto,
                                Impuesto = impuesto,
                                Total = total
                            });
                            allTargetDocIds.Add(docId);
                        }
                        else
                        {
                            // Documento activo guardado para cruzar si tiene partidas canceladas
                            activeDocDict[docId] = (docDate, docSerie, folio, "PUBLICO GENERAL", docType, neto, impuesto, total);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error al leer registro en POS10008 para cancelaciones");
                    }
                }
            }

            // 2. Leer POS10010 para extraer partidas de cancelaciones completas y detectar cancelaciones parciales
            var docDetails = new Dictionary<string, List<CancellationDetailDto>>();
            var partialCancelledDocDetails = new Dictionary<string, List<CancellationDetailDto>>();
            var productIds = new HashSet<string>();

            if (!string.IsNullOrEmpty(pos10010Path) && File.Exists(pos10010Path))
            {
                try
                {
                    using var reader10 = _readerFactory.CreateReader(pos10010Path);
                    ValidateColumns(reader10, "POS10010", "CIDDOCUM01", "CIDPRODU01", "CUNIDADES", "CPRECIO", "CTOTAL", "CCANCELADO");

                    while (reader10.Read())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            var docId = reader10.GetStringSafe("CIDDOCUM01")?.Trim() ?? string.Empty;
                            if (string.IsNullOrEmpty(docId)) continue;

                            var prodId = reader10.GetStringSafe("CIDPRODU01")?.Trim() ?? string.Empty;
                            var movCancelado = reader10.GetInt32Safe("CCANCELADO");
                            var units = (double)reader10.GetDecimalSafe("CUNIDADES");
                            var price = reader10.GetDecimalSafe("CPRECIO");
                            var movTotal = reader10.GetDecimalSafe("CTOTAL");

                            if (!string.IsNullOrEmpty(prodId))
                            {
                                productIds.Add(prodId);
                            }

                            var detail = new CancellationDetailDto
                            {
                                ProductId = prodId,
                                Units = units,
                                Price = price,
                                Total = movTotal
                            };

                            // Si pertenece a documento con cancelación completa
                            if (allTargetDocIds.Contains(docId))
                            {
                                if (!docDetails.TryGetValue(docId, out var list))
                                {
                                    list = new List<CancellationDetailDto>();
                                    docDetails[docId] = list;
                                }
                                list.Add(detail);
                            }
                            // Si pertenece a documento activo pero la partida está cancelada -> Cancelación Parcial
                            else if (movCancelado == 1 && activeDocDict.ContainsKey(docId))
                            {
                                if (!partialCancelledDocDetails.TryGetValue(docId, out var pList))
                                {
                                    pList = new List<CancellationDetailDto>();
                                    partialCancelledDocDetails[docId] = pList;
                                }
                                pList.Add(detail);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Error al leer registro en POS10010 para cancelaciones");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al leer POS10010 para cancelaciones");
                }
            }

            // 3. Obtener nombres de productos desde MGW10005
            var productsFilePath = config.Mgw10005Path;
            var productNames = new Dictionary<string, string>();
            if (productIds.Any() && !string.IsNullOrEmpty(productsFilePath) && File.Exists(productsFilePath))
            {
                try
                {
                    using var productReader = _readerFactory.CreateReader(productsFilePath);
                    ValidateColumns(productReader, "MGW10005", "CIDPRODU01", "CNOMBREP01");

                    while (productReader.Read())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var pId = productReader.GetStringSafe("CIDPRODU01")?.Trim() ?? string.Empty;

                        if (productIds.Contains(pId))
                        {
                            var pName = productReader.GetStringSafe("CNOMBREP01");
                            productNames[pId] = pName;
                        }

                        if (productNames.Count == productIds.Count) break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error al leer nombres de productos en MGW10005 para cancelaciones");
                }
            }

            // 4. Armar lista completa de cancelaciones
            // 4a. Cancelaciones completas
            foreach (var doc in completeCancelledDocs)
            {
                var details = docDetails.TryGetValue(doc.IdDocumento, out var dList) ? dList : new List<CancellationDetailDto>();
                foreach (var d in details)
                {
                    if (productNames.TryGetValue(d.ProductId, out var name))
                    {
                        d.ProductName = name;
                    }
                }

                cancellations.Add(doc with
                {
                    PartidasCanceladasCount = details.Count,
                    Detalles = details
                });
            }

            // 4b. Cancelaciones parciales
            foreach (var kvp in partialCancelledDocDetails)
            {
                var docId = kvp.Key;
                var pDetails = kvp.Value;
                var parentDoc = activeDocDict[docId];

                foreach (var d in pDetails)
                {
                    if (productNames.TryGetValue(d.ProductId, out var name))
                    {
                        d.ProductName = name;
                    }
                }

                var totalCancelado = pDetails.Sum(d => d.Total);
                string docTypeName = parentDoc.DocType switch
                {
                    35 => "Venta POS",
                    36 => "Devolución POS",
                    17 => "Pedido POS",
                    _ => "Documento POS"
                };

                cancellations.Add(new CancellationReportItemDto
                {
                    IdDocumento = docId,
                    Fecha = parentDoc.Fecha,
                    Serie = parentDoc.Serie,
                    Folio = parentDoc.Folio,
                    TipoCancelacion = "Parcial",
                    TipoDocumento = docTypeName,
                    Cliente = parentDoc.Cliente,
                    Neto = totalCancelado / 1.16m, // Estimación base para la partida cancelada
                    Impuesto = totalCancelado - (totalCancelado / 1.16m),
                    Total = totalCancelado,
                    PartidasCanceladasCount = pDetails.Count,
                    Detalles = pDetails
                });
            }

            // 5. Filtrar por tipo si aplica ("Completa" / "Parcial")
            if (!string.IsNullOrWhiteSpace(tipo) && !string.Equals(tipo, "Todas", StringComparison.OrdinalIgnoreCase))
            {
                cancellations = cancellations.Where(x => string.Equals(x.TipoCancelacion, tipo.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return cancellations.OrderByDescending(x => x.Fecha).ThenBy(x => x.Folio).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte de cancelaciones desde POS10008");
            throw;
        }
    }

    private void ValidateColumns(IFoxProDataReader reader, string fileName, params string[] columns)
    {
        var missingColumns = new List<string>();
        foreach (var col in columns)
        {
            try
            {
                reader.GetOrdinal(col);
            }
            catch
            {
                missingColumns.Add(col);
            }
        }

        if (missingColumns.Any())
        {
            throw new InvalidOperationException($"Faltan las siguientes columnas en {fileName}: {string.Join(", ", missingColumns)}. Verifique la estructura del archivo.");
        }
    }
}
