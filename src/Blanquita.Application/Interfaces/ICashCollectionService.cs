using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface ICashCollectionService
{
    Task<CashCollectionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCollectionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCollectionDto>> SearchAsync(SearchCashCollectionRequest request, CancellationToken cancellationToken = default);
    Task<CashCollectionDto> CreateAsync(CreateCashCollectionDto dto, CancellationToken cancellationToken = default);
    Task MarkAsCutAsync(string cashRegisterName, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
