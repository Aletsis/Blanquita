using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Repositories;

public class ConciliacionCorteRepository : IConciliacionCorteRepository
{
    private readonly BlanquitaDbContext _context;

    public ConciliacionCorteRepository(BlanquitaDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ConciliacionCorte conciliacion, CancellationToken cancellationToken = default)
    {
        await _context.ConciliacionCortes.AddAsync(conciliacion, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ConciliacionCorte conciliacion, CancellationToken cancellationToken = default)
    {
        _context.ConciliacionCortes.Update(conciliacion);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ConciliacionCorte?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ConciliacionCortes
            .Include(c => c.Salidas)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ConciliacionCorte?> GetByAperturaIdAsync(int aperturaId, CancellationToken cancellationToken = default)
    {
        return await _context.ConciliacionCortes
            .Include(c => c.Salidas)
            .FirstOrDefaultAsync(c => c.AperturaId == aperturaId, cancellationToken);
    }

    public async Task<IEnumerable<ConciliacionCorte>> GetByBranchAndDateAsync(string branchName, DateTime date, CancellationToken cancellationToken = default)
    {
        var localStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Local);
        var startUtc = localStart.ToUniversalTime();
        var endUtc = startUtc.AddDays(1);

        return await _context.ConciliacionCortes
            .Include(c => c.Salidas)
            .Where(c => c.Sucursal == branchName && c.Fecha >= startUtc && c.Fecha < endUtc)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<int>> GetAlreadyConciliatedShiftIdsAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var localStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Local);
        var startUtc = localStart.ToUniversalTime();
        var endUtc = startUtc.AddDays(1);

        return await _context.ConciliacionCortes
            .Where(c => c.Fecha >= startUtc && c.Fecha < endUtc)
            .Select(c => c.AperturaId)
            .ToListAsync(cancellationToken);
    }
}
