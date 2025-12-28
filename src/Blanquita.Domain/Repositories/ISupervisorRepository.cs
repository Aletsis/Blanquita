using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Repositories;

public interface ISupervisorRepository
{
    Task<Supervisor?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Supervisor?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Supervisor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Supervisor>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Supervisor>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Supervisor supervisor, CancellationToken cancellationToken = default);
    Task UpdateAsync(Supervisor supervisor, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
}
