using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces.Repositories;

/// <summary>
/// Repositorio para acceder a cortes de caja desde FoxPro.
/// </summary>
public interface IFoxProCashCutRepository
{
    /// <summary>
    /// Obtiene cortes de caja diarios por fecha y sucursal.
    /// </summary>
    /// <param name="date">Fecha de los cortes</param>
    /// <param name="branchId">ID de la sucursal</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de cortes de caja</returns>
    Task<IEnumerable<CashCutDto>> GetDailyCashCutsAsync(
        DateTime date, 
        int branchId, 
        CancellationToken cancellationToken = default);
}
