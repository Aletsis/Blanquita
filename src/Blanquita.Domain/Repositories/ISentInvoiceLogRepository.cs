using Blanquita.Domain.Entities;

namespace Blanquita.Domain.Repositories;

public interface ISentInvoiceLogRepository
{
    Task<bool> ExistsAsync(string clientCode, string fileName, CancellationToken cancellationToken = default);
    Task AddAsync(SentInvoiceLog log, CancellationToken cancellationToken = default);
}
