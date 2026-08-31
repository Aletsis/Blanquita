using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Repositories;

public interface IConciliacionCorteRepository
{
    Task AddAsync(ConciliacionCorte conciliacion, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConciliacionCorte conciliacion, CancellationToken cancellationToken = default);
    Task<ConciliacionCorte?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ConciliacionCorte?> GetByAperturaIdAsync(int aperturaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConciliacionCorte>> GetByBranchAndDateAsync(string branchName, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<int>> GetAlreadyConciliatedShiftIdsAsync(DateTime date, CancellationToken cancellationToken = default);
}
