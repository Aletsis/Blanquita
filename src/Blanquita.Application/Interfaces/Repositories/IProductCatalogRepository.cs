using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces.Repositories;

/// <summary>
/// Repositorio para acceder al catálogo de productos.
/// </summary>
public interface IProductCatalogRepository
{
    /// <summary>
    /// Busca un producto por su código.
    /// </summary>
    /// <param name="code">Código del producto</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El producto si se encuentra, null en caso contrario</returns>
    Task<ProductDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca productos que coincidan con el término de búsqueda en varios campos.
    /// </summary>
    Task<IEnumerable<ProductSearchDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
