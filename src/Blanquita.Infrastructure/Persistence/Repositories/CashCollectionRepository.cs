using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Repositories;

public class CashCollectionRepository : ICashCollectionRepository
{
    private readonly BlanquitaDbContext _context;

    public CashCollectionRepository(BlanquitaDbContext context)
    {
        _context = context;
    }

    public async Task<CashCollection?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.CashCollections
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CashCollection?> GetByFolioAsync(int folio, CancellationToken cancellationToken = default)
    {
        return await _context.CashCollections
            .FirstOrDefaultAsync(c => c.Folio == folio, cancellationToken);
    }

    public async Task<IEnumerable<CashCollection>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CashCollections
            .AsNoTracking()
            .OrderByDescending(c => c.CollectionDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CashCollection>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.CashCollections
            .AsNoTracking()
            .Where(c => c.CollectionDateTime >= startDate && c.CollectionDateTime <= endDate)
            .OrderByDescending(c => c.CollectionDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CashCollection>> GetByCashRegisterAsync(string cashRegisterName, CancellationToken cancellationToken = default)
    {
        return await _context.CashCollections
            .AsNoTracking()
            .Where(c => c.CashRegisterName == cashRegisterName)
            .OrderByDescending(c => c.CollectionDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CashCollection>> GetForCashCutAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CashCollections
            .AsNoTracking()
            .Where(c => c.IsForCashCut)
            .OrderByDescending(c => c.CollectionDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CashCollection cashCollection, CancellationToken cancellationToken = default)
    {
        await _context.CashCollections.AddAsync(cashCollection, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CashCollection cashCollection, CancellationToken cancellationToken = default)
    {
        _context.CashCollections.Update(cashCollection);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashCollection = await GetByIdAsync(id, cancellationToken);
        if (cashCollection != null)
        {
            _context.CashCollections.Remove(cashCollection);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetNextFolioAsync(string cashRegisterName, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var startUtc = today.Kind == DateTimeKind.Utc ? today : today.ToUniversalTime();
        var endUtc = startUtc.AddDays(1);

        var maxFolio = await _context.CashCollections
            .Where(c => c.CashRegisterName == cashRegisterName 
                   && !c.IsForCashCut 
                   && c.CollectionDateTime >= startUtc 
                   && c.CollectionDateTime < endUtc)
            .MaxAsync(c => (int?)c.Folio, cancellationToken);
        
        return (maxFolio ?? 0) + 1;
    }

    public async Task<IEnumerable<CashCollection>> GetPendingCollectionsByRegisterAsync(string cashRegisterName, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var startUtc = today.Kind == DateTimeKind.Utc ? today : today.ToUniversalTime();
        var endUtc = startUtc.AddDays(1);

        return await _context.CashCollections
            .Where(c => c.CashRegisterName == cashRegisterName 
                   && !c.IsForCashCut 
                   && c.CollectionDateTime >= startUtc 
                   && c.CollectionDateTime < endUtc)
            .OrderBy(c => c.CollectionDateTime)
            .ToListAsync(cancellationToken);
    }
}
