using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Repositories;

public interface ILabelDesignRepository
{
    Task<IEnumerable<LabelDesign>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LabelDesign?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LabelDesign?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(LabelDesign design, CancellationToken cancellationToken = default);
    Task UpdateAsync(LabelDesign design, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task RemoveAllDefaultsAsync(CancellationToken cancellationToken = default);
}
