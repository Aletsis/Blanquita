using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces.Repositories;

/// <summary>
/// Repositorio para acceder a productos desde FoxPro.
/// </summary>
public interface IFoxProProductRepository
{
    /// <summary>
    /// Busca un producto por su código.
    /// </summary>
    /// <param name="code">Código del producto</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El producto si se encuentra, null en caso contrario</returns>
    Task<ProductDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
