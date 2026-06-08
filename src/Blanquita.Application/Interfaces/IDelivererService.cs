using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface IDelivererService
{
    Task<DelivererDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DelivererDto?> GetByEmployeeNumberAsync(int employeeNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<DelivererDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DelivererDto>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DelivererDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<DelivererDto> CreateAsync(CreateDelivererDto dto, CancellationToken cancellationToken = default);
    Task<DelivererDto> UpdateAsync(UpdateDelivererDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<DelivererDto>> GetPagedAsync(SearchDelivererRequest request, CancellationToken cancellationToken = default);
}
