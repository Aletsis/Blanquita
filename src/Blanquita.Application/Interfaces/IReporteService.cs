using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Servicio para gestionar la persistencia de reportes históricos en archivos JSON.
/// Proporciona operaciones CRUD sobre reportes almacenados localmente.
/// </summary>
public interface IReporteService
{
    /// <summary>
    /// Guarda un nuevo reporte en el sistema de archivos.
    /// </summary>
    Task GuardarReporteAsync(ReporteDto reporte);
    
    /// <summary>
    /// Obtiene todos los reportes almacenados.
    /// </summary>
    Task<List<ReporteDto>> ObtenerReportesAsync();
    
    /// <summary>
    /// Obtiene un reporte específico por su ID.
    /// </summary>
    Task<ReporteDto?> ObtenerReportePorIdAsync(int id);
    
    /// <summary>
    /// Elimina un reporte del sistema de archivos.
    /// </summary>
    Task EliminarReporteAsync(int id);
    
    /// <summary>
    /// Busca reportes aplicando filtros opcionales.
    /// </summary>
    Task<List<ReporteDto>> BuscarReportesAsync(string? sucursal = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    
    /// <summary>
    /// Actualiza un reporte existente.
    /// </summary>
    Task ActualizarReporteAsync(ReporteDto reporte);
}
