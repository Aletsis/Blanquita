using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface ICashCutService
{
    Task<CashCutDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCutDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCutDto>> SearchAsync(SearchCashCutRequest request, CancellationToken cancellationToken = default);
    Task<CashCutDto> CreateAsync(CreateCashCutDto dto, CancellationToken cancellationToken = default);
    Task<CashCutDto> ProcessCashCutAsync(ProcessCashCutRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
