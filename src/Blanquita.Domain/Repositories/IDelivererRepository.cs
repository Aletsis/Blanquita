using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Repositories;

public interface IDelivererRepository
{
    Task<Deliverer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Deliverer?> GetByEmployeeNumberAsync(int employeeNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Deliverer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Deliverer>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Deliverer>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Deliverer deliverer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Deliverer deliverer, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int employeeNumber, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Deliverer> Items, int TotalCount)> GetPagedAsync(
        string? searchTerm,
        int page,
        int pageSize,
        string? sortColumn,
        bool sortAscending,
        CancellationToken cancellationToken = default);
}
