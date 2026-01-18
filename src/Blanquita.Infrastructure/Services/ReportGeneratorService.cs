using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Servicio para generar datos de reportes de corte de caja.
/// Coordina la obtención de datos desde FoxPro y su procesamiento para generar reportes.
/// </summary>
public class ReportGeneratorService : IReportGeneratorService
{
    private readonly IFoxProCashCutRepository _cashCutRepository;
    private readonly IFoxProDocumentRepository _documentRepository;
    private readonly IDbfStringParser _dbfParser;
    private readonly ILogger<ReportGeneratorService> _logger;

    public ReportGeneratorService(
        IFoxProCashCutRepository cashCutRepository,
        IFoxProDocumentRepository documentRepository,
        IDbfStringParser dbfParser,
        ILogger<ReportGeneratorService> logger)
    {
        _cashCutRepository = cashCutRepository;
        _documentRepository = documentRepository;
        _dbfParser = dbfParser;
        _logger = logger;
    }

    public async Task<List<ReportRowDto>> GenerarReportDataAsync(BranchDto sucursal, DateTime fecha)
    {
        var result = new List<ReportRowDto>();
        
        _logger.LogInformation("Iniciando generación de reporte para {Sucursal} en fecha {Fecha:dd/MM/yyyy}", sucursal.Name, fecha);
        _logger.LogDebug("Series - Cliente: {SerieCliente}, Global: {SerieGlobal}, Devolucion: {SerieDevolucion}", sucursal.SeriesCliente, sucursal.SeriesGlobal, sucursal.SeriesDevolucion);

        try
        {
            // Paso 1: Obtener todos los cortes de la fecha especificada
            _logger.LogInformation("Buscando cortes en fecha {Fecha:dd/MM/yyyy}", fecha);
            // Assuming branchId 1 or ignored for local files logic
            var cortes = await _cashCutRepository.GetDailyCashCutsAsync(fecha, 1);
            _logger.LogInformation("Total de cortes encontrados: {Count}", cortes.Count());

            if (!cortes.Any())
            {
                _logger.LogInformation("No hay cortes para la fecha {Fecha:dd/MM/yyyy}", fecha);
                return result;
            }

            // Paso 2: Obtener todos los documentos de la fecha y sucursal
            _logger.LogInformation("Obteniendo documentos para {Sucursal}", sucursal.Name);
            var documentos = await _documentRepository.GetByDateAndBranchAsync(fecha, 1);
            _logger.LogInformation("Total de documentos encontrados: {Count}", documentos.Count());

            // Paso 3: Crear índice de documentos por clave compuesta para búsquedas O(1)
            var documentosIndex = documentos
                .GroupBy(d => (d.IdDocumento, d.Serie, d.Folio))
                .ToDictionary(g => g.Key, g => g.First());

            _logger.LogDebug("Índice de documentos creado con {Count} entradas únicas", documentosIndex.Count);

            // Paso 4: Agrupar cortes por caja para consolidar datos
            var cortesPorCaja = cortes.GroupBy(c => c.CashRegisterName);
            _logger.LogInformation("Cajas únicas encontradas: {Count}", cortesPorCaja.Count());

            foreach (var grupoCaja in cortesPorCaja)
            {
                var nombreCaja = grupoCaja.Key;
                var cortesDelaCaja = grupoCaja.ToList();
                
                _logger.LogDebug("Procesando caja '{NombreCaja}' con {NumCortes} corte(s)", nombreCaja, cortesDelaCaja.Count);

                decimal facturado = 0;
                decimal ventaGlobal = 0;
                decimal devolucion = 0;

                // Procesar facturas (Cliente) - usar CTEXTOEX03 para identificar la caja
                // Solo se procesan UNA VEZ por caja, sin importar cuántos cortes tenga
                var docsCliente = documentos.Where(d =>
                    d.Serie == sucursal.SeriesCliente &&
                    d.CajaTexto.Equals(nombreCaja, StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var doc in docsCliente)
                {
                    facturado += doc.Total;
                    _logger.LogDebug("  + Factura Cliente: {Serie}-{Folio} (Caja: {CajaTexto}) = {Total:C2}", doc.Serie, doc.Folio, doc.CajaTexto, doc.Total);
                }

                // Procesar facturas (Global) - consolidar de TODOS los cortes de esta caja
                // Usar HashSet para evitar duplicados si un documento aparece en múltiples cortes
                var idsDocumentosGlobalesProcesados = new HashSet<(string IdDocumento, string Serie, string Folio)>();
                
                foreach (var corte in cortesDelaCaja)
                {
                    var docsFactGlobal = _dbfParser.ParsearDocumentos(corte.RawInvoices);
                    foreach (var doc in docsFactGlobal)
                    {
                        var clave = (doc.IdDocumento, doc.Serie, doc.Folio);
                        
                        // Solo procesar si no lo hemos visto antes
                        if (!idsDocumentosGlobalesProcesados.Contains(clave))
                        {
                            // Búsqueda O(1) en el diccionario en lugar de O(n) con FirstOrDefault
                            if (documentosIndex.TryGetValue(clave, out var docEncontrado) 
                                && docEncontrado.Serie == sucursal.SeriesGlobal)
                            {
                                ventaGlobal += docEncontrado.Total;
                                idsDocumentosGlobalesProcesados.Add(clave);
                                _logger.LogDebug("  + Venta Global: {Serie}-{Folio} = {Total:C2}", doc.Serie, doc.Folio, docEncontrado.Total);
                            }
                        }
                    }
                }

                // Procesar devoluciones - consolidar de TODOS los cortes de esta caja
                // Usar HashSet para evitar duplicados si un documento aparece en múltiples cortes
                var idsDocumentosDevolucionesProcesados = new HashSet<(string IdDocumento, string Serie, string Folio)>();
                
                foreach (var corte in cortesDelaCaja)
                {
                    var docsDev = _dbfParser.ParsearDocumentos(corte.RawReturns);
                    foreach (var doc in docsDev)
                    {
                        var clave = (doc.IdDocumento, doc.Serie, doc.Folio);
                        
                        // Solo procesar si no lo hemos visto antes
                        if (!idsDocumentosDevolucionesProcesados.Contains(clave))
                        {
                            // Búsqueda O(1) en el diccionario en lugar de O(n) con FirstOrDefault
                            if (documentosIndex.TryGetValue(clave, out var docEncontrado) 
                                && docEncontrado.Serie == sucursal.SeriesDevolucion)
                            {
                                devolucion += docEncontrado.Total;
                                idsDocumentosDevolucionesProcesados.Add(clave);
                                _logger.LogDebug("  - Devolución: {Serie}-{Folio} = {Total:C2}", doc.Serie, doc.Folio, docEncontrado.Total);
                            }
                        }
                    }
                }

                decimal total = facturado + ventaGlobal - devolucion;
                _logger.LogDebug("  Totales consolidados caja '{NombreCaja}': Fact={Facturado:C2}, VG={VentaGlobal:C2}, Dev={Devolucion:C2}, Total={Total:C2}", nombreCaja, facturado, ventaGlobal, devolucion, total);

                if (facturado > 0 || ventaGlobal > 0 || devolucion > 0)
                {
                    result.Add(new ReportRowDto
                    {
                        Fecha = fecha.ToString("dd/MM/yyyy"),
                        Caja = nombreCaja,
                        Facturado = facturado,
                        Devolucion = devolucion,
                        VentaGlobal = ventaGlobal,
                        Total = total
                    });
                    _logger.LogDebug("  ✓ Registro consolidado agregado al reporte");
                }
            }

            _logger.LogInformation("Generación de reporte completada. Total de registros: {Count}", result.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción durante la generación");
            throw new InvalidOperationException($"Error al generar reporte: {ex.Message}", ex);
        }

        return result;
    }
}
