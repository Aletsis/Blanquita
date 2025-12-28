using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Repositories;

public interface ICashierRepository
{
    Task<Cashier?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Cashier?> GetByEmployeeNumberAsync(int employeeNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cashier>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Cashier>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cashier>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Cashier cashier, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cashier cashier, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int employeeNumber, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Cashier> Items, int TotalCount)> GetPagedAsync(string? searchTerm, int page, int pageSize, string? sortColumn, bool sortAscending, CancellationToken cancellationToken = default);
}
