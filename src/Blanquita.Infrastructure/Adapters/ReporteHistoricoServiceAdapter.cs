using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Entities;
using Blanquita.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Adapters;

/// <summary>
/// Adaptador que implementa IReporteHistoricoService usando el ReporteService de persistencia.
/// Permite migrar gradualmente de la arquitectura antigua a Clean Architecture.
/// Convierte entre DTOs de persistencia y entidades de dominio.
/// </summary>
public class ReporteHistoricoServiceAdapter : IReporteHistoricoService
{
    private readonly IReporteService _reporteService;
    private readonly ILogger<ReporteHistoricoServiceAdapter> _logger;

    public ReporteHistoricoServiceAdapter(
        IReporteService reporteService,
        ILogger<ReporteHistoricoServiceAdapter> logger)
    {
        _reporteService = reporteService ?? throw new ArgumentNullException(nameof(reporteService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task GuardarReporteAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Guardando reporte histórico para sucursal {Sucursal}", reporte.Sucursal.Nombre);
        
        var reporteDto = MapearADto(reporte);
        await _reporteService.GuardarReporteAsync(reporteDto);
        
        _logger.LogInformation("Reporte guardado exitosamente con ID {Id}", reporteDto.Id);
    }

    public async Task<List<ReporteHistorico>> ObtenerReportesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo todos los reportes históricos");
        
        var reportes = await _reporteService.ObtenerReportesAsync();
        var resultado = reportes.Select(MapearAEntidad).ToList();
        
        _logger.LogInformation("Se obtuvieron {Count} reportes históricos", resultado.Count);
        return resultado;
    }

    public async Task<ReporteHistorico?> ObtenerReportePorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo reporte histórico con ID {Id}", id);
        
        var reporte = await _reporteService.ObtenerReportePorIdAsync(id);
        
        if (reporte == null)
        {
            _logger.LogWarning("No se encontró reporte con ID {Id}", id);
            return null;
        }
        
        return MapearAEntidad(reporte);
    }

    public async Task EliminarReporteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando reporte histórico con ID {Id}", id);
        
        await _reporteService.EliminarReporteAsync(id);
        
        _logger.LogInformation("Reporte eliminado exitosamente");
    }

    public async Task<List<ReporteHistorico>> BuscarReportesAsync(
        BuscarReportesRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Buscando reportes - Sucursal: {Sucursal}, FechaInicio: {FechaInicio}, FechaFin: {FechaFin}",
            request.Sucursal?.Nombre ?? "Todas",
            request.FechaInicio,
            request.FechaFin);

        var (inicio, fin) = request.GetNormalizedDateRange();
        var sucursalNombre = request.Sucursal?.Nombre;

        var reportes = await _reporteService.BuscarReportesAsync(sucursalNombre, inicio, fin);
        var resultado = reportes.Select(MapearAEntidad).ToList();

        _logger.LogInformation("Se encontraron {Count} reportes que coinciden con los criterios", resultado.Count);
        return resultado;
    }

    public async Task ActualizarReporteAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando reporte histórico con ID {Id}", reporte.Id);
        
        var reporteDto = MapearADto(reporte);
        await _reporteService.ActualizarReporteAsync(reporteDto);
        
        _logger.LogInformation("Reporte actualizado exitosamente");
    }

    // Métodos de mapeo privados
    private ReporteHistorico MapearAEntidad(ReporteDto dto)
    {
        var sucursal = Sucursal.FromNombre(dto.Sucursal) ?? Sucursal.Himno;
        
        var detalles = dto.Detalles?.Select(d => DetalleReporte.Crear(
            d.Fecha,
            d.Caja,
            d.Facturado,
            d.Devolucion,
            d.VentaGlobal,
            d.Total
        )).ToList() ?? new List<DetalleReporte>();

        var reporte = ReporteHistorico.Crear(
            sucursal,
            dto.Fecha,
            dto.TotalSistema,
            dto.TotalCorteManual,
            detalles
        );

        // Usar reflexión para establecer propiedades privadas (temporal, hasta migrar a repositorio)
        var idProperty = typeof(ReporteHistorico).BaseType?.GetProperty("Id");
        idProperty?.SetValue(reporte, dto.Id);

        if (!string.IsNullOrEmpty(dto.Notas))
        {
            reporte.ActualizarNotas(dto.Notas);
        }

        var fechaGeneracionField = typeof(ReporteHistorico).GetField(
            "<FechaGeneracion>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fechaGeneracionField?.SetValue(reporte, dto.FechaGeneracion);

        return reporte;
    }

    private ReporteDto MapearADto(ReporteHistorico entidad)
    {
        return new ReporteDto
        {
            Id = entidad.Id,
            Sucursal = entidad.Sucursal.Nombre,
            Fecha = entidad.Fecha,
            TotalSistema = entidad.TotalSistema,
            TotalCorteManual = entidad.TotalCorteManual,
            Diferencia = entidad.Diferencia,
            Notas = entidad.Notas,
            FechaGeneracion = entidad.FechaGeneracion,
            Detalles = entidad.Detalles.Select(d => new ReportRowDto
            {
                Fecha = d.Fecha,
                Caja = d.Caja,
                Facturado = d.Facturado,
                Devolucion = d.Devolucion,
                VentaGlobal = d.VentaGlobal,
                Total = d.Total
            }).ToList()
        };
    }
}
