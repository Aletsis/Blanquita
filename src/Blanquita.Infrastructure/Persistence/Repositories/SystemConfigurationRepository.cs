using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Repositories;

public class SystemConfigurationRepository : ISystemConfigurationRepository
{
    private readonly BlanquitaDbContext _context;

    public SystemConfigurationRepository(BlanquitaDbContext context)
    {
        _context = context;
    }

    public async Task<SystemConfiguration?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SystemConfigurations.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(SystemConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await _context.SystemConfigurations.AddAsync(configuration, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SystemConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var local = _context.SystemConfigurations.Local.FirstOrDefault(e => e.Id == configuration.Id);
        if (local != null)
        {
            _context.Entry(local).State = EntityState.Detached;
        }
        _context.SystemConfigurations.Update(configuration);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
