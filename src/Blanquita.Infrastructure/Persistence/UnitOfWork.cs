using Blanquita.Application.Interfaces;
using Blanquita.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence;

/// <summary>
/// Implementación de <see cref="IUnitOfWork"/> sobre PostgreSQL usando transacciones de EF Core
/// y bloqueos consultivos (pg_advisory_xact_lock), que se liberan automáticamente al terminar la transacción.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly BlanquitaDbContext _context;

    public UnitOfWork(BlanquitaDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteLockedAsync<T>(string lockKey, Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        // Si ya hay una transacción en curso, el bloqueo se suma a ella
        if (_context.Database.CurrentTransaction != null)
        {
            await AcquireLockAsync(lockKey, cancellationToken);
            return await operation(cancellationToken);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await AcquireLockAsync(lockKey, cancellationToken);
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            // Descartar entidades rastreadas con cambios que ya no existen en la BD
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private Task AcquireLockAsync(string lockKey, CancellationToken cancellationToken)
    {
        return _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtext({lockKey}))", cancellationToken);
    }
}
