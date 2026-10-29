using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface ICashCollectionService
{
    Task<CashCollectionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCollectionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CashCollectionDto>> SearchAsync(SearchCashCollectionRequest request, CancellationToken cancellationToken = default);
    Task<CashCollectionDto> CreateAsync(CreateCashCollectionDto dto, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtiene las recolecciones del día (hora local del servidor) que aún no se han incluido en un corte.
    /// Es exactamente el conjunto que <see cref="MarkAsCutAsync"/> marca como cortado.
    /// </summary>
    Task<IEnumerable<CashCollectionDto>> GetPendingForCutAsync(string cashRegisterName, CancellationToken cancellationToken = default);
    Task MarkAsCutAsync(string cashRegisterName, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
