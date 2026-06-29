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

    /// <summary>
    /// Obtiene todas las series de venta asociadas a una sucursal en FoxPro.
    /// </summary>
    /// <param name="branchCode">Código de la sucursal (ej. B6, B7, B8, B9, B10)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de series de venta únicas</returns>
    Task<IEnumerable<string>> GetSeriesByBranchCodeAsync(string branchCode, CancellationToken cancellationToken = default);
}

