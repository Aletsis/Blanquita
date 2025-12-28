using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface IFoxProReportService
{
    Task<IEnumerable<CashCutDto>> GetDailyCashCutsAsync(DateTime date, int branchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> GetDocumentsByDateAndBranchAsync(DateTime date, int branchId, CancellationToken cancellationToken = default);
    BranchSeriesDto GetBranchSeries(string branchName);
    Task<bool> VerifyConnectionAsync(CancellationToken cancellationToken = default);
    Task<DiagnosticoResultado> DiagnosticarArchivoAsync(string path, List<string>? expectedColumns = null, CancellationToken cancellationToken = default);
    Task<List<Dictionary<string, object>>> ObtenerRegistrosMuestraAsync(string path, int cantidad, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByCodeAsync(string code, CancellationToken cancellationToken = default);
}

