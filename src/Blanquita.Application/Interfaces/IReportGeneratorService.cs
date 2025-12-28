using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

/// <summary>
/// Servicio para generar datos de reportes de corte de caja.
/// Coordina la obtención de datos desde FoxPro y su procesamiento.
/// </summary>
public interface IReportGeneratorService
{
    /// <summary>
    /// Genera los datos del reporte para una sucursal y fecha específica.
    /// </summary>
    /// <param name="sucursal">Nombre de la sucursal</param>
    /// <param name="fecha">Fecha del reporte</param>
    /// <returns>Lista de filas del reporte con los totales por caja</returns>
    Task<List<ReportRowDto>> GenerarReportDataAsync(string sucursal, DateTime fecha);
}
