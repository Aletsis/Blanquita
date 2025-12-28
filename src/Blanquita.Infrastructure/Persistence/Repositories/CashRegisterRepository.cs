using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Repositories;

public class CashRegisterRepository : ICashRegisterRepository
{
    private readonly BlanquitaDbContext _context;

    public CashRegisterRepository(BlanquitaDbContext context)
    {
        _context = context;
    }

    public async Task<CashRegister?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.CashRegisters
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CashRegister?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.CashRegisters
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<CashRegister>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CashRegisters.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CashRegister>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var id = BranchId.Create(branchId);
        return await _context.CashRegisters
            .Where(c => c.BranchId == id)
            .ToListAsync(cancellationToken);
    }

    public async Task<CashRegister?> GetLastRegisterByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var id = BranchId.Create(branchId);
        return await _context.CashRegisters
            .FirstOrDefaultAsync(c => c.BranchId == id && c.IsLastRegister, cancellationToken);
    }

    public async Task AddAsync(CashRegister cashRegister, CancellationToken cancellationToken = default)
    {
        await _context.CashRegisters.AddAsync(cashRegister, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CashRegister cashRegister, CancellationToken cancellationToken = default)
    {
        _context.CashRegisters.Update(cashRegister);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cashRegister = await GetByIdAsync(id, cancellationToken);
        if (cashRegister != null)
        {
            _context.CashRegisters.Remove(cashRegister);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.CashRegisters
            .AnyAsync(c => c.Name == name, cancellationToken);
    }
}
