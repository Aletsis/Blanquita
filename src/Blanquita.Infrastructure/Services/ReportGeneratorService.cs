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

    public async Task<List<ReportRowDto>> GenerarReportDataAsync(string sucursal, DateTime fecha)
    {
        var result = new List<ReportRowDto>();
        
        // Get Branch Series Config
        var series = SeriesDocumentoSucursal.ObtenerPorNombre(sucursal);

        _logger.LogInformation("Iniciando generación de reporte para {Sucursal} en fecha {Fecha:dd/MM/yyyy}", sucursal, fecha);
        _logger.LogDebug("Series - Cliente: {SerieCliente}, Global: {SerieGlobal}, Devolucion: {SerieDevolucion}", series.SerieCliente, series.SerieGlobal, series.SerieDevolucion);

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
            _logger.LogInformation("Obteniendo documentos para {Sucursal}", sucursal);
            var documentos = await _documentRepository.GetByDateAndBranchAsync(fecha, 1);
            _logger.LogInformation("Total de documentos encontrados: {Count}", documentos.Count());

            // Paso 3: Procesar cada corte
            foreach (var corte in cortes)
            {
                _logger.LogDebug("Procesando corte - Caja ID: {IdCaja}, Serie: '{SerieCaja}'", corte.CashRegisterId, corte.CashRegisterName);

                decimal facturado = 0;
                decimal ventaGlobal = 0;
                decimal devolucion = 0;

                // Procesar facturas (Cliente) - usar CTEXTOEX03 para identificar la caja
                var docsCliente = documentos.Where(d =>
                    d.Serie == series.SerieCliente &&
                    d.CajaTexto.Equals(corte.CashRegisterName, StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var doc in docsCliente)
                {
                    facturado += doc.Total;
                    _logger.LogDebug("  + Factura Cliente: {Serie}-{Folio} (Caja: {CajaTexto}) = {Total:C2}", doc.Serie, doc.Folio, doc.CajaTexto, doc.Total);
                }

                // Procesar facturas (Global) - usar campo CFACTURA del corte
                var docsFactGlobal = _dbfParser.ParsearDocumentos(corte.RawInvoices);
                foreach (var doc in docsFactGlobal)
                {
                    var docEncontrado = documentos.FirstOrDefault(d =>
                        d.IdDocumento == doc.IdDocumento &&
                        d.Serie == doc.Serie &&
                        d.Folio == doc.Folio &&
                        d.Serie == series.SerieGlobal);

                    if (docEncontrado != null)
                    {
                        ventaGlobal += docEncontrado.Total;
                        _logger.LogDebug("  + Venta Global: {Serie}-{Folio} = {Total:C2}", doc.Serie, doc.Folio, docEncontrado.Total);
                    }
                }

                // Procesar devoluciones - usar campo CDEVOLUCIO del corte
                var docsDev = _dbfParser.ParsearDocumentos(corte.RawReturns);
                foreach (var doc in docsDev)
                {
                    var docEncontrado = documentos.FirstOrDefault(d =>
                        d.IdDocumento == doc.IdDocumento &&
                        d.Serie == doc.Serie &&
                        d.Folio == doc.Folio &&
                        d.Serie == series.SerieDevolucion);

                    if (docEncontrado != null)
                    {
                        devolucion += docEncontrado.Total;
                        _logger.LogDebug("  - Devolución: {Serie}-{Folio} = {Total:C2}", doc.Serie, doc.Folio, docEncontrado.Total);
                    }
                }

                decimal total = facturado + ventaGlobal - devolucion;
                _logger.LogDebug("  Totales caja '{SerieCaja}': Fact={Facturado:C2}, VG={VentaGlobal:C2}, Dev={Devolucion:C2}, Total={Total:C2}", corte.CashRegisterName, facturado, ventaGlobal, devolucion, total);

                if (facturado > 0 || ventaGlobal > 0 || devolucion > 0)
                {
                    result.Add(new ReportRowDto
                    {
                        Fecha = fecha.ToString("dd/MM/yyyy"),
                        Caja = corte.CashRegisterName,
                        Facturado = facturado,
                        Devolucion = devolucion,
                        VentaGlobal = ventaGlobal,
                        Total = total
                    });
                    _logger.LogDebug("  ✓ Registro agregado al reporte");
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
