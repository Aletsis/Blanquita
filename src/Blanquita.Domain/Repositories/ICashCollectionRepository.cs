using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Repositories;

public interface ICashCollectionRepository
{
    Task<CashCollection?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CashCollection?> GetByFolioAsync(int folio, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCollection>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCollection>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCollection>> GetByCashRegisterAsync(string cashRegisterName, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCollection>> GetForCashCutAsync(CancellationToken cancellationToken = default);
    Task AddAsync(CashCollection cashCollection, CancellationToken cancellationToken = default);
    Task UpdateAsync(CashCollection cashCollection, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> GetNextFolioAsync(string cashRegisterName, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCollection>> GetPendingCollectionsByRegisterAsync(string cashRegisterName, CancellationToken cancellationToken = default);
}
