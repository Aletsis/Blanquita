using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Repositories;

public interface ICashCutRepository
{
    Task<CashCut?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCut>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCut>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCut>> GetByBranchAsync(string branchName, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCut>> GetByCashRegisterAsync(string cashRegisterName, CancellationToken cancellationToken = default);
    Task AddAsync(CashCut cashCut, CancellationToken cancellationToken = default);
    Task UpdateAsync(CashCut cashCut, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
