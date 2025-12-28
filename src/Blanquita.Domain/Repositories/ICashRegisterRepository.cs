using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Repositories;

public interface ICashRegisterRepository
{
    Task<CashRegister?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CashRegister?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashRegister>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CashRegister>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task<CashRegister?> GetLastRegisterByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task AddAsync(CashRegister cashRegister, CancellationToken cancellationToken = default);
    Task UpdateAsync(CashRegister cashRegister, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
}
