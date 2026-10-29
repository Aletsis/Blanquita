using Blanquita.Application.Interfaces;

namespace Blanquita.Application.Tests.Helpers;

/// <summary>
/// Implementación de prueba que ejecuta la operación directamente y registra las llaves de bloqueo usadas.
/// </summary>
public class PassThroughUnitOfWork : IUnitOfWork
{
    public List<string> LockKeys { get; } = new();

    public Task<T> ExecuteLockedAsync<T>(string lockKey, Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        LockKeys.Add(lockKey);
        return operation(cancellationToken);
    }
}
