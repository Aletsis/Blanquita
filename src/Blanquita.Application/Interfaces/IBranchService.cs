using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface IBranchService
{
    Task<IEnumerable<BranchDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
