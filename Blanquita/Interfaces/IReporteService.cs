using Blanquita.Models;

namespace Blanquita.Interfaces
{
    public interface IReporteService
    {
        Task GuardarReporteAsync(Reporte reporte);
        Task<List<Reporte>> ObtenerReportesAsync();
        Task<Reporte?> ObtenerReportePorIdAsync(int id);
        Task EliminarReporteAsync(int id);
        Task<List<Reporte>> BuscarReportesAsync(string? sucursal = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        Task ActualizarReporteAsync(Reporte reporte);
    }
}