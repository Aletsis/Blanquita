using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface ISupervisorService
{
    Task<SupervisorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SupervisorDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<SupervisorDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SupervisorDto>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SupervisorDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<SupervisorDto> CreateAsync(CreateSupervisorDto dto, CancellationToken cancellationToken = default);
    Task<SupervisorDto> UpdateAsync(UpdateSupervisorDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<SupervisorDto>> GetPagedAsync(SearchSupervisorRequest request, CancellationToken cancellationToken = default);
}
