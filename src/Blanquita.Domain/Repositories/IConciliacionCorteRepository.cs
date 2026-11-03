using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Repositories;

public interface IConciliacionCorteRepository
{
    Task AddAsync(ConciliacionCorte conciliacion, CancellationToken cancellationToken = default);
    /// <summary>
    /// Guarda los cambios solo si la fila sigue en <paramref name="expectedVersion"/>;
    /// si otro usuario la modificó antes lanza <see cref="Exceptions.ConcurrencyConflictException"/>.
    /// </summary>
    Task UpdateAsync(ConciliacionCorte conciliacion, uint expectedVersion, CancellationToken cancellationToken = default);
    Task<ConciliacionCorte?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsForShiftAsync(int aperturaId, string sucursal, CancellationToken cancellationToken = default);
    Task<ConciliacionCorte?> GetByAperturaIdAsync(int aperturaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConciliacionCorte>> GetByBranchAndDateAsync(string branchName, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<int>> GetAlreadyConciliatedShiftIdsAsync(DateTime date, CancellationToken cancellationToken = default);
}
