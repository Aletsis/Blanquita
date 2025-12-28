using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface IBranchService
{
    Task<IEnumerable<BranchDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BranchDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
