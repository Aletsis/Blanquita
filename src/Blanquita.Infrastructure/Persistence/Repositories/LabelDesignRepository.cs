using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Repositories;

public class LabelDesignRepository : ILabelDesignRepository
{
    private readonly BlanquitaDbContext _context;

    public LabelDesignRepository(BlanquitaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LabelDesign>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LabelDesigns
            .Include(d => d.Elements)
            .Where(d => d.IsActive)
            .OrderByDescending(d => d.IsDefault)
            .ThenBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<LabelDesign?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.LabelDesigns
            .Include(d => d.Elements)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<LabelDesign?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LabelDesigns
            .Include(d => d.Elements)
            .FirstOrDefaultAsync(d => d.IsDefault && d.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.LabelDesigns.AnyAsync(d => d.Name == name, cancellationToken);
    }

    public async Task AddAsync(LabelDesign design, CancellationToken cancellationToken = default)
    {
        await _context.LabelDesigns.AddAsync(design, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LabelDesign design, CancellationToken cancellationToken = default)
    {
        _context.LabelDesigns.Update(design);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var design = await _context.LabelDesigns.FindAsync(new object?[] { id }, cancellationToken);
        if (design != null)
        {
            _context.LabelDesigns.Remove(design);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveAllDefaultsAsync(CancellationToken cancellationToken = default)
    {
        var defaultDesigns = await _context.LabelDesigns
            .Where(d => d.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var design in defaultDesigns)
        {
            design.RemoveDefault();
        }
        
        // Note: SaveChangesAsync will be called by the service or we can call it here if we want immediate effect
        await _context.SaveChangesAsync(cancellationToken);
    }
}
