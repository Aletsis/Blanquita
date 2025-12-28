using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface ICashierService
{
    Task<CashierDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CashierDto?> GetByEmployeeNumberAsync(int employeeNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashierDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CashierDto>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashierDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<CashierDto> CreateAsync(CreateCashierDto dto, CancellationToken cancellationToken = default);
    Task<CashierDto> UpdateAsync(UpdateCashierDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<CashierDto>> GetPagedAsync(SearchCashierRequest request, CancellationToken cancellationToken = default);
}
