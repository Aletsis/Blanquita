using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface ICashRegisterService
{
    Task<CashRegisterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CashRegisterDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashRegisterDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CashRegisterDto>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task<CashRegisterDto> CreateAsync(CreateCashRegisterDto dto, CancellationToken cancellationToken = default);
    Task<CashRegisterDto> UpdateAsync(UpdateCashRegisterDto dto, CancellationToken cancellationToken = default);
    Task<CashRegisterDto?> GetBackupRegisterAsync(int currentRegisterId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<CashRegisterDto>> GetPagedAsync(SearchCashRegisterRequest request, CancellationToken cancellationToken = default);
}
