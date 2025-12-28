using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Repositories;

public class CashCutRepository : ICashCutRepository
{
    private readonly BlanquitaDbContext _context;

    public CashCutRepository(BlanquitaDbContext context)
    {
        _context = context;
    }

    public async Task<CashCut?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.CashCuts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<CashCut>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CashCuts
            .OrderByDescending(c => c.CutDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CashCut>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.CashCuts
            .Where(c => c.CutDateTime >= startDate && c.CutDateTime <= endDate)
            .OrderByDescending(c => c.CutDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CashCut>> GetByBranchAsync(string branchName, CancellationToken cancellationToken = default)
    {
        return await _context.CashCuts
            .Where(c => c.BranchName == branchName)
            .OrderByDescending(c => c.CutDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CashCut>> GetByCashRegisterAsync(string cashRegisterName, CancellationToken cancellationToken = default)
    {
        return await _context.CashCuts
            .Where(c => c.CashRegisterName == cashRegisterName)
            .OrderByDescending(c => c.CutDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CashCut cashCut, CancellationToken cancellationToken = default)
    {
        await _context.CashCuts.AddAsync(cashCut, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CashCut cashCut, CancellationToken cancellationToken = default)
    {
        _context.CashCuts.Update(cashCut);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashCut = await GetByIdAsync(id, cancellationToken);
        if (cashCut != null)
        {
            _context.CashCuts.Remove(cashCut);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
