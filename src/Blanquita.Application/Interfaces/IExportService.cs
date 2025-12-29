using Blanquita.Domain.Entities;

namespace Blanquita.Application.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Data", CancellationToken cancellationToken = default);
    Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string title = "Report", CancellationToken cancellationToken = default);
    Task<byte[]> ExportReporteToExcelAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default);
    Task<byte[]> ExportReporteToPdfAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default);
}
