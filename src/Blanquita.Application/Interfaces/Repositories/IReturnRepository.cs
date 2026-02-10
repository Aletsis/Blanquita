using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces.Repositories;

/// <summary>
/// Repository for accessing return transactions.
/// </summary>
public interface IReturnRepository
{
    /// <summary>
    /// Finds a return by its series and folio.
    /// </summary>
    /// <param name="series">Series of the return</param>
    /// <param name="folio">Folio of the return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The return if found, null otherwise</returns>
    Task<ReturnDto?> GetBySeriesAndFolioAsync(string series, string folio, CancellationToken cancellationToken = default);
}
