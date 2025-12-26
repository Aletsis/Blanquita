using Blanquita.Exceptions;
using Blanquita.Interfaces;
using Blanquita.Models;
using Blanquita.Services.Parsing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blanquita.Services
{
    public class ReportGeneratorService : IReportGeneratorService
    {
        private readonly IFoxProService _foxProService;
        private readonly IDbfStringParser _dbfParser;
        private readonly ILogger<ReportGeneratorService> _logger;

        public ReportGeneratorService(
            IFoxProService foxProService,
            IDbfStringParser dbfParser,
            ILogger<ReportGeneratorService> logger)
        {
            _foxProService = foxProService;
            _dbfParser = dbfParser;
            _logger = logger;
        }

        public async Task<List<ReportRow>> GenerarReportDataAsync(string sucursal, DateTime fecha)
        {
            var result = new List<ReportRow>();
            var series = _foxProService.GetBranchSeries(sucursal);

            _logger.LogDebug("=== Iniciando generación de reporte ===");
            _logger.LogDebug("Sucursal: {Sucursal}", sucursal);
            _logger.LogDebug("Fecha: {Fecha:dd/MM/yyyy}", fecha);
            _logger.LogDebug("Series - Cliente: {SerieCliente}, Global: {SerieGlobal}, Devolucion: {SerieDevolucion}", series.Cliente, series.Global, series.Devolucion);

            try
            {
                // Paso 1: Obtener todos los cortes de la fecha especificada
                _logger.LogDebug("Paso 1: Buscando cortes en fecha {Fecha:dd/MM/yyyy}...", fecha);
                var cortes = await _foxProService.GetCortesDelDiaAsync(fecha);
                _logger.LogDebug("Total de cortes encontrados: {Count}", cortes.Count);

                if (!cortes.Any())
                {
                    _logger.LogDebug("No hay cortes para esta fecha");
                    return result;
                }

                // Paso 2: Obtener todos los documentos de la fecha y sucursal
                _logger.LogDebug("Paso 2: Obteniendo documentos de la fecha para series de {Sucursal}...", sucursal);
                var documentos = await _foxProService.GetDocumentosPorFechaYSucursalAsync(fecha, series);
                _logger.LogDebug("Total de documentos encontrados: {Count}", documentos.Count);

                // Paso 3: Procesar cada corte
                foreach (var corte in cortes)
                {
                    _logger.LogDebug("Procesando corte - Caja ID: {IdCaja}, Serie: '{SerieCaja}'", corte.IdCaja, corte.SerieCaja);

                    decimal facturado = 0;
                    decimal ventaGlobal = 0;
                    decimal devolucion = 0;

                    // Procesar facturas (Cliente) - usar CTEXTOEX03 para identificar la caja
                    var docsCliente = documentos.Where(d =>
                        d.Serie == series.Cliente &&
                        d.CajaTexto.Equals(corte.SerieCaja, StringComparison.OrdinalIgnoreCase)).ToList();

                    foreach (var doc in docsCliente)
                    {
                        facturado += doc.Total;
                        _logger.LogDebug("  + Factura Cliente: {Serie}-{Folio} (Caja: {CajaTexto}) = {Total:C2}", doc.Serie, doc.Folio, doc.CajaTexto, doc.Total);
                    }

                    // Procesar facturas (Global) - usar campo CFACTURA del corte
                    var docsFactGlobal = _dbfParser.ParsearDocumentos(corte.Facturas);
                    foreach (var doc in docsFactGlobal)
                    {
                        var docEncontrado = documentos.FirstOrDefault(d =>
                            d.IdDocumento == doc.IdDocumento &&
                            d.Serie == doc.Serie &&
                            d.Folio == doc.Folio &&
                            d.Serie == series.Global);

                        if (docEncontrado != null)
                        {
                            ventaGlobal += docEncontrado.Total;
                            _logger.LogDebug("  + Venta Global: {Serie}-{Folio} = {Total:C2}", doc.Serie, doc.Folio, docEncontrado.Total);
                        }
                    }

                    // Procesar devoluciones - usar campo CDEVOLUCIO del corte
                    var docsDev = _dbfParser.ParsearDocumentos(corte.Devoluciones);
                    foreach (var doc in docsDev)
                    {
                        var docEncontrado = documentos.FirstOrDefault(d =>
                            d.IdDocumento == doc.IdDocumento &&
                            d.Serie == doc.Serie &&
                            d.Folio == doc.Folio &&
                            d.Serie == series.Devolucion);

                        if (docEncontrado != null)
                        {
                            devolucion += docEncontrado.Total;
                            _logger.LogDebug("  - Devolución: {Serie}-{Folio} = {Total:C2}", doc.Serie, doc.Folio, docEncontrado.Total);
                        }
                    }

                    decimal total = facturado + ventaGlobal - devolucion;
                    _logger.LogDebug("  Totales caja '{SerieCaja}': Fact={Facturado:C2}, VG={VentaGlobal:C2}, Dev={Devolucion:C2}, Total={Total:C2}", corte.SerieCaja, facturado, ventaGlobal, devolucion, total);

                    if (facturado > 0 || ventaGlobal > 0 || devolucion > 0)
                    {
                        result.Add(new ReportRow
                        {
                            Fecha = fecha.ToString("dd/MM/yyyy"),
                            Caja = corte.SerieCaja,
                            Facturado = facturado,
                            Devolucion = devolucion,
                            VentaGlobal = ventaGlobal,
                            Total = total
                        });
                        _logger.LogDebug("  ✓ Registro agregado al reporte");
                    }
                }

                _logger.LogDebug("=== Fin de generación ===");
                _logger.LogDebug("Total de registros en el reporte: {Count}", result.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción durante la generación");
                throw new ReportGenerationException($"Error al generar reporte: {ex.Message}", ex);
            }

            return result;
        }
    }
}
