using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Repositories;

public class SentInvoiceLogRepository : ISentInvoiceLogRepository
{
    private readonly BlanquitaDbContext _context;

    public SentInvoiceLogRepository(BlanquitaDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(string clientCode, string fileName, CancellationToken cancellationToken = default)
    {
        return await _context.SentInvoiceLogs
            .AnyAsync(l => l.ClientCode == clientCode && l.FileName == fileName, cancellationToken);
    }

    public async Task AddAsync(SentInvoiceLog log, CancellationToken cancellationToken = default)
    {
        await _context.SentInvoiceLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
