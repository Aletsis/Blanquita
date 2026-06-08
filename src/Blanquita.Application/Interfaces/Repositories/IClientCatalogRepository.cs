using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces.Repositories;

public interface IClientCatalogRepository
{
    Task<IEnumerable<ClientSearchDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<ClientSearchDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClientSearchDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientSearchDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClientSearchDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
}
