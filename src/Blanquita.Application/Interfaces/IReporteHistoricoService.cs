using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Servicio de aplicación para gestionar reportes históricos.
/// Define las operaciones disponibles para trabajar con reportes de corte de caja.
/// </summary>
public interface IReporteHistoricoService
{
    /// <summary>
    /// Guarda un nuevo reporte histórico en el sistema.
    /// </summary>
    Task GuardarReporteAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los reportes históricos disponibles.
    /// </summary>
    Task<List<ReporteHistorico>> ObtenerReportesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un reporte histórico específico por su ID.
    /// </summary>
    Task<ReporteHistorico?> ObtenerReportePorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un reporte histórico del sistema.
    /// </summary>
    Task EliminarReporteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca reportes históricos aplicando los filtros especificados.
    /// </summary>
    Task<List<ReporteHistorico>> BuscarReportesAsync(
        BuscarReportesRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un reporte histórico existente.
    /// </summary>
    Task ActualizarReporteAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default);
}
