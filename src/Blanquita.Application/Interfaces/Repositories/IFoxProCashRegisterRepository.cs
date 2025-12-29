namespace Blanquita.Application.Interfaces.Repositories;

/// <summary>
/// Repositorio para acceder a cajas registradoras desde FoxPro.
/// </summary>
public interface IFoxProCashRegisterRepository
{
    /// <summary>
    /// Obtiene el nombre de una caja registradora por su ID.
    /// </summary>
    /// <param name="cashRegisterId">ID de la caja registradora</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Nombre de la caja registradora, o string vacío si no se encuentra</returns>
    Task<string> GetNameByIdAsync(int cashRegisterId, CancellationToken cancellationToken = default);
}
