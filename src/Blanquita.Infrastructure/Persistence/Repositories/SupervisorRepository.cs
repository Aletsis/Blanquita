using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Repositories;

public class SupervisorRepository : ISupervisorRepository
{
    private readonly BlanquitaDbContext _context;

    public SupervisorRepository(BlanquitaDbContext context)
    {
        _context = context;
    }

    public async Task<Supervisor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Supervisors
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Supervisor?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Supervisors
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Supervisor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Supervisors.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Supervisor>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var id = BranchId.Create(branchId);
        return await _context.Supervisors
            .Where(s => s.BranchId == id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Supervisor>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Supervisors
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Supervisor supervisor, CancellationToken cancellationToken = default)
    {
        await _context.Supervisors.AddAsync(supervisor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Supervisor supervisor, CancellationToken cancellationToken = default)
    {
        _context.Supervisors.Update(supervisor);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var supervisor = await GetByIdAsync(id, cancellationToken);
        if (supervisor != null)
        {
            _context.Supervisors.Remove(supervisor);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Supervisors
            .AnyAsync(s => s.Name == name, cancellationToken);
    }
}
