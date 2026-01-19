using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.Exceptions;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
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
            using var reader = _readerFactory.CreateReader(filePath);

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var docDate = reader.GetDateTimeSafe("CFECHA");

                    if (docDate.Date == date.Date)
                    {
                        documents.Add(FoxProDocumentMapper.MapToDto(reader));
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
            using (var reader = _readerFactory.CreateReader(documentsPath))
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
