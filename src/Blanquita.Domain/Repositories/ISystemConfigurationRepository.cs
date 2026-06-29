using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Repositories;

public interface ISystemConfigurationRepository
{
    Task<SystemConfiguration?> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SystemConfiguration configuration, CancellationToken cancellationToken = default);
    Task UpdateAsync(SystemConfiguration configuration, CancellationToken cancellationToken = default);
    Task AddAuditLogAsync(SystemConfigurationAuditLog auditLog, CancellationToken cancellationToken = default);
}
