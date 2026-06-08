using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;

public class FoxProPedidoRepository : IFoxProPedidoRepository
{
    private readonly IConfiguracionService _configService;
    private readonly IFoxProReaderFactory _readerFactory;
    private readonly IClientCatalogRepository _clientRepository;
    private readonly IProductCatalogRepository _productRepository;
    private readonly ILogger<FoxProPedidoRepository> _logger;

    public FoxProPedidoRepository(
        IConfiguracionService configService,
        IFoxProReaderFactory readerFactory,
        IClientCatalogRepository clientRepository,
        IProductCatalogRepository productRepository,
        ILogger<FoxProPedidoRepository> logger)
    {
        _configService = configService;
        _readerFactory = readerFactory;
        _clientRepository = clientRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<PedidoDto>> SearchPedidosAsync(
        DateTime date, 
        IEnumerable<string> series, 
        string? comanda = null, 
        CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Pos10008Path;

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("Archivo POS10008 no configurado o no existe: {FilePath}", filePath);
            return Enumerable.Empty<PedidoDto>();
        }

        var pedidos = new List<PedidoDto>();
        var seriesList = series.Select(s => s.Trim().ToUpper()).ToList();

        try
        {
            using var reader = _readerFactory.CreateReverseReader(filePath);

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var cfecha = reader.GetDateTimeSafe("CFECHA");
                
                // Como leemos de atrás hacia adelante, si la fecha es más antigua, terminamos
                if (cfecha.Date < date.Date) break;
                // Si la fecha es más reciente (ej. buscamos ayer, pero estamos leyendo hoy), la saltamos
                if (cfecha.Date > date.Date) continue;

                var cseriedo01 = reader.GetStringSafe("CSERIEDO01")?.Trim() ?? string.Empty;
                if (seriesList.Any() && !seriesList.Contains(cseriedo01.ToUpper())) continue;

                var ctextoex01 = reader.GetStringSafe("CTEXTOEX01") ?? string.Empty;
                var (extractedRuta, extractedNomina) = ExtractRutaAndNomina(ctextoex01);

                var ctextoex02 = reader.GetStringSafe("CTEXTOEX02") ?? string.Empty;
                var extractedComanda = ExtractComanda(ctextoex02);

                if (!string.IsNullOrEmpty(comanda))
                {
                    if (!CompareComanda(extractedComanda, comanda)) continue;
                }

                var pedido = new PedidoDto
                {
                    IdDocumento = reader.GetStringSafe("CIDDOCUM01"),
                    Folio = $"{cseriedo01}-{reader.GetStringSafe("CFOLIO")}",
                    Fecha = cfecha,
                    Comanda = extractedComanda,
                    Ruta = extractedRuta,
                    Total = reader.GetDecimalSafe("CTOTAL"),
                    NetAmount = reader.GetDecimalSafe("CNETO"),
                    TaxAmount = reader.GetDecimalSafe("CIMPUESTO1"),
                    ClienteId = reader.GetInt32Safe("CIDCLIEN01"),
                    Status = DetermineStatus(reader)
                };

                pedidos.Add(pedido);
            }

            // Cargar nombres de clientes
            if (pedidos.Any())
            {
                var uniqueClientIds = pedidos.Select(p => p.ClienteId).Distinct().ToList();
                var clients = await _clientRepository.GetByIdsAsync(uniqueClientIds, cancellationToken);
                var clientDict = clients.ToDictionary(c => c.Id, c => c.Name);

                foreach (var p in pedidos)
                {
                    if (clientDict.TryGetValue(p.ClienteId, out var name))
                        p.Cliente = name;
                    else
                        p.Cliente = "CLIENTE DESCONOCIDO";
                }
            }

            return pedidos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar pedidos en POS10008");
            throw;
        }
    }

    public async Task<IEnumerable<PedidoDto>> SearchRutasAsync(
        DateTime date, 
        IEnumerable<string> series, 
        string? ruta = null, 
        string? repartidorNomina = null, 
        CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Pos10008Path;

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("Archivo POS10008 no configurado o no existe: {FilePath}", filePath);
            return Enumerable.Empty<PedidoDto>();
        }

        var pedidos = new List<PedidoDto>();
        var seriesList = series.Select(s => s.Trim().ToUpper()).ToList();

        try
        {
            using var reader = _readerFactory.CreateReverseReader(filePath);

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var cfecha = reader.GetDateTimeSafe("CFECHA");
                
                // Como leemos de atrás hacia adelante, si la fecha es más antigua, terminamos
                if (cfecha.Date < date.Date) break;
                // Si la fecha es más reciente (ej. buscamos ayer, pero estamos leyendo hoy), la saltamos
                if (cfecha.Date > date.Date) continue;

                var cseriedo01 = reader.GetStringSafe("CSERIEDO01")?.Trim() ?? string.Empty;
                if (seriesList.Any() && !seriesList.Contains(cseriedo01.ToUpper())) continue;

                var ctextoex01 = reader.GetStringSafe("CTEXTOEX01") ?? string.Empty;
                var (extractedRuta, extractedNomina) = ExtractRutaAndNomina(ctextoex01);

                // Filtrar por Ruta si se proporcionó
                if (!string.IsNullOrEmpty(ruta))
                {
                    if (!extractedRuta.Equals(ruta, StringComparison.OrdinalIgnoreCase)) continue;
                }

                // Filtrar por Repartidor (Nómina) si se proporcionó
                if (!string.IsNullOrEmpty(repartidorNomina))
                {
                    if (extractedNomina != repartidorNomina) continue;
                }

                var ctextoex02 = reader.GetStringSafe("CTEXTOEX02") ?? string.Empty;
                var extractedComanda = ExtractComanda(ctextoex02);

                _logger.LogInformation("Ruta encontrada: Folio {Folio}, Ruta {Ruta}, Repartidor {Rep}", reader.GetStringSafe("CFOLIO"), extractedRuta, extractedNomina);

                var pedido = new PedidoDto
                {
                    IdDocumento = reader.GetStringSafe("CIDDOCUM01"),
                    Folio = $"{cseriedo01}-{reader.GetStringSafe("CFOLIO")}",
                    Fecha = cfecha,
                    Comanda = extractedComanda,
                    Ruta = extractedRuta,
                    Total = reader.GetDecimalSafe("CTOTAL"),
                    NetAmount = reader.GetDecimalSafe("CNETO"),
                    TaxAmount = reader.GetDecimalSafe("CIMPUESTO1"),
                    ClienteId = reader.GetInt32Safe("CIDCLIEN01"),
                    Status = DetermineStatus(reader)
                };

                pedidos.Add(pedido);
            }

            // Cargar nombres de clientes
            if (pedidos.Any())
            {
                var uniqueClientIds = pedidos.Select(p => p.ClienteId).Distinct().ToList();
                var clients = await _clientRepository.GetByIdsAsync(uniqueClientIds, cancellationToken);
                var clientDict = clients.ToDictionary(c => c.Id, c => c.Name);

                foreach (var p in pedidos)
                {
                    if (clientDict.TryGetValue(p.ClienteId, out var name))
                        p.Cliente = name;
                    else
                        p.Cliente = "CLIENTE DESCONOCIDO";
                }
            }

            return pedidos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar rutas en POS10008");
            throw;
        }
    }

    public async Task<PedidoDto?> GetBySeriesAndFolioAsync(string series, string folio, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Pos10008Path;

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("Archivo POS10008 no configurado o no existe: {FilePath}", filePath);
            return null;
        }

        try
        {
            // Podríamos leer de atrás hacia adelante ya que es probable que los pedidos buscados sean recientes.
            using var reader = _readerFactory.CreateReverseReader(filePath);

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var cseriedo01 = reader.GetStringSafe("CSERIEDO01")?.Trim() ?? string.Empty;
                var cfolio = reader.GetStringSafe("CFOLIO")?.Trim() ?? string.Empty;

                if (string.Equals(cseriedo01, series?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(cfolio, folio?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    var ctextoex01 = reader.GetStringSafe("CTEXTOEX01") ?? string.Empty;
                    var (extractedRuta, extractedNomina) = ExtractRutaAndNomina(ctextoex01);

                    var ctextoex02 = reader.GetStringSafe("CTEXTOEX02") ?? string.Empty;
                    var extractedComanda = ExtractComanda(ctextoex02);

                    var pedido = new PedidoDto
                    {
                        IdDocumento = reader.GetStringSafe("CIDDOCUM01"),
                        Folio = $"{cseriedo01}-{cfolio}",
                        Fecha = reader.GetDateTimeSafe("CFECHA"),
                        Comanda = extractedComanda,
                        Ruta = extractedRuta,
                        Total = reader.GetDecimalSafe("CTOTAL"),
                        NetAmount = reader.GetDecimalSafe("CNETO"),
                        TaxAmount = reader.GetDecimalSafe("CIMPUESTO1"),
                        ClienteId = reader.GetInt32Safe("CIDCLIEN01"),
                        Status = DetermineStatus(reader)
                    };

                    // Cargar datos del cliente
                    var clients = await _clientRepository.GetByIdsAsync(new List<int> { pedido.ClienteId }, cancellationToken);
                    var client = clients.FirstOrDefault();
                    if (client != null)
                    {
                        pedido.Cliente = client.Name;
                        pedido.ClienteCodigo = client.Code;
                        var address = client.Addresses?.FirstOrDefault();
                        if (address != null)
                        {
                            pedido.Domicilio = $"{address.Street} {address.ExteriorNumber} {address.InteriorNumber}".Trim();
                            pedido.Colonia = address.Colony;
                        }
                    }
                    else
                    {
                        pedido.Cliente = "CLIENTE DESCONOCIDO";
                    }

                    // Cargar Items
                    var items = await GetPedidoItemsAsync(pedido.IdDocumento, cancellationToken);
                    pedido.Items = items.ToList();

                    return pedido;
                }
            }

            _logger.LogInformation("Pedido no encontrado: Series {Series}, Folio {Folio}", series, folio);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar pedido Series {Series}, Folio {Folio} en FoxPro", series, folio);
            throw;
        }
    }

    private (string Ruta, string Nomina) ExtractRutaAndNomina(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return (string.Empty, string.Empty);
        
        var parts = texto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return (string.Empty, string.Empty);

        var ruta = parts[0]; // Ej: R01
        var nomina = parts[1]; // Ej: 1343

        return (ruta, nomina);
    }

    public async Task<IEnumerable<PedidoItemDto>> GetPedidoItemsAsync(string idDocumento, CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Pos10010Path;

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("Archivo POS10010 no configurado o no existe: {FilePath}", filePath);
            return Enumerable.Empty<PedidoItemDto>();
        }

        var items = new List<PedidoItemInternal>();

        try
        {
            using var reader = _readerFactory.CreateReverseReader(filePath);

            bool foundBlock = false;
            int nonMatchingRecordsSinceLastMatch = 0;

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentIdDoc = reader.GetStringSafe("CIDDOCUM01");
                
                if (currentIdDoc != idDocumento)
                {
                    if (foundBlock)
                    {
                        nonMatchingRecordsSinceLastMatch++;
                        // Si ya pasamos un margen seguro de registros sin encontrar más partidas
                        // de este documento, podemos dar por terminada la búsqueda.
                        if (nonMatchingRecordsSinceLastMatch > 5000)
                        {
                            break;
                        }
                    }
                    
                    continue;
                }

                foundBlock = true;
                nonMatchingRecordsSinceLastMatch = 0; // Resetear el contador al encontrar una partida válida

                // Intentar obtener el ID del producto de varias columnas posibles
                var prodId02 = reader.GetInt32Safe("CIDPRODU02");
                var prodId01 = reader.GetInt32Safe("CIDPRODU01");
                var prodIdS = reader.GetInt32Safe("CIDPRODU");
                
                // En AdminPAQ POS, el ID del producto suele ser 02. El 01 suele ser el ID del movimiento.
                var finalProdId = prodId02 != 0 ? prodId02 : (prodId01 != 0 ? prodId01 : prodIdS);

                // Intentar obtener el precio de varias columnas posibles
                var precioC0 = reader.GetDecimalSafe("CPRECIOC01");
                var precioStd = reader.GetDecimalSafe("CPRECIO");
                var precio01 = reader.GetDecimalSafe("CPRECIO01");
                var finalPrecio = precioC0 != 0 ? precioC0 : (precioStd != 0 ? precioStd : precio01);

                var cantidad = reader.GetDecimalSafe("CUNIDADES");
                var neto = reader.GetDecimalSafe("CNETO");
                
                // Si el neto es 0, lo calculamos como respaldo
                var finalNeto = neto != 0 ? neto : (cantidad * finalPrecio);

                items.Add(new PedidoItemInternal
                {
                    ProductoId = finalProdId,
                    Cantidad = cantidad,
                    Precio = finalPrecio,
                    Subtotal = finalNeto,
                    Impuesto = reader.GetDecimalSafe("CIMPUESTO1"),
                    PorcentajeImpuesto = reader.GetDecimalSafe("CPORCENT01")
                });
            }

            if (!items.Any()) 
            {
                _logger.LogWarning("No se encontraron partidas para el documento {IdDocumento} en POS10010", idDocumento);
                return Enumerable.Empty<PedidoItemDto>();
            }

            // Cargar datos de productos
            var productIds = items.Select(i => i.ProductoId).Distinct().ToList();
            _logger.LogDebug("Buscando {Count} productos únicos: {Ids}", productIds.Count, string.Join(", ", productIds));
            
            var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
            var productDict = products.ToDictionary(p => p.Id);
            
            _logger.LogDebug("Productos encontrados en catálogo: {Count}", products.Count());

            return items.Select(i => new PedidoItemDto
            {
                Codigo = productDict.TryGetValue(i.ProductoId, out var p) ? p.Code : "S/C",
                Descripcion = productDict.TryGetValue(i.ProductoId, out var p2) ? p2.Name : $"PRODUCTO NO ENCONTRADO ({i.ProductoId})",
                Cantidad = i.Cantidad,
                Precio = i.Precio,
                Subtotal = i.Subtotal,
                Impuesto = i.Impuesto,
                PorcentajeImpuesto = i.PorcentajeImpuesto
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partidas del pedido {IdDocumento}", idDocumento);
            throw;
        }
    }

    private class PedidoItemInternal
    {
        public int ProductoId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal PorcentajeImpuesto { get; set; }
    }

    private string ExtractComanda(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return string.Empty;
        var parts = texto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return string.Empty;
        
        var firstPart = parts[0];
        
        // Caso 'F52': Letra pegada al número
        if (firstPart.Length > 1 && char.IsLetter(firstPart[0]) && char.IsDigit(firstPart[1]))
        {
            return firstPart.Substring(1);
        }
        
        // Caso 'F 52': Letra separada por espacio del número
        if (firstPart.Length == 1 && char.IsLetter(firstPart[0]) && parts.Length > 1)
        {
            return parts[1];
        }
        
        return firstPart; 
    }

    private bool CompareComanda(string extracted, string filter)
    {
        if (int.TryParse(extracted, out int extInt) && int.TryParse(filter, out int filInt))
        {
            return extInt == filInt;
        }
        return extracted.Equals(filter, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<PedidoDto>> GetSalesReportByFiltersAsync(
        DateTime startDate,
        DateTime endDate,
        TimeSpan startTime,
        TimeSpan endTime,
        IEnumerable<string> series,
        CancellationToken cancellationToken = default)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var filePath = config.Pos10008Path;

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("Archivo POS10008 no configurado o no existe: {FilePath}", filePath);
            return Enumerable.Empty<PedidoDto>();
        }

        var pedidos = new List<PedidoDto>();
        var seriesList = series.Select(s => s.Trim().ToUpper()).ToList();

        try
        {
            using var reader = _readerFactory.CreateReverseReader(filePath);

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var cfecha = reader.GetDateTimeSafe("CFECHA");

                if (cfecha.Date < startDate.Date) break;
                if (cfecha.Date > endDate.Date) continue;

                var cseriedo01 = reader.GetStringSafe("CSERIEDO01")?.Trim() ?? string.Empty;
                if (seriesList.Any() && !seriesList.Contains(cseriedo01.ToUpper())) continue;

                var chora = reader.GetStringSafe("CHORA")?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(chora))
                {
                    if (TimeSpan.TryParse(chora, out var docTime))
                    {
                        if (docTime < startTime || docTime > endTime)
                        {
                            continue;
                        }
                    }
                }

                var ctextoex01 = reader.GetStringSafe("CTEXTOEX01") ?? string.Empty;
                var (extractedRuta, extractedNomina) = ExtractRutaAndNomina(ctextoex01);

                var ctextoex02 = reader.GetStringSafe("CTEXTOEX02") ?? string.Empty;
                var extractedComanda = ExtractComanda(ctextoex02);

                var pedido = new PedidoDto
                {
                    IdDocumento = reader.GetStringSafe("CIDDOCUM01"),
                    Folio = $"{cseriedo01}-{reader.GetStringSafe("CFOLIO")}",
                    Fecha = cfecha,
                    Hora = chora,
                    Comanda = extractedComanda,
                    Ruta = extractedRuta,
                    Total = reader.GetDecimalSafe("CTOTAL"),
                    NetAmount = reader.GetDecimalSafe("CNETO"),
                    TaxAmount = reader.GetDecimalSafe("CIMPUESTO1"),
                    ClienteId = reader.GetInt32Safe("CIDCLIEN01"),
                    Status = DetermineStatus(reader)
                };

                pedidos.Add(pedido);
            }

            if (pedidos.Any())
            {
                var uniqueClientIds = pedidos.Select(p => p.ClienteId).Distinct().ToList();
                var clients = await _clientRepository.GetByIdsAsync(uniqueClientIds, cancellationToken);
                var clientDict = clients.ToDictionary(c => c.Id, c => c.Name);

                foreach (var p in pedidos)
                {
                    if (clientDict.TryGetValue(p.ClienteId, out var name))
                        p.Cliente = name;
                    else
                        p.Cliente = "CLIENTE DESCONOCIDO";
                }
            }

            return pedidos.OrderBy(p => p.Fecha).ThenBy(p => p.Hora).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de ventas por rango en POS10008");
            throw;
        }
    }

    private string DetermineStatus(IFoxProDataReader reader)
    {
        var cancelado = reader.GetInt32Safe("CCANCELADO");
        var devuelto = reader.GetInt32Safe("CDEVUELTO");
        var afectado = reader.GetInt32Safe("CAFECTADO");
        var impreso = reader.GetInt32Safe("CIMPRESO");

        if (cancelado == 1) return "Cancelado";
        if (devuelto == 1) return "Devuelto";
        if (afectado == 1) return "Cobrada";
        if (impreso == 1) return "Impresa";
        
        return "Pendiente";
    }
}
