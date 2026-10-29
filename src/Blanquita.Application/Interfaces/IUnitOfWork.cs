namespace Blanquita.Application.Interfaces;

/// <summary>
/// Coordina operaciones que deben ejecutarse de forma atómica y serializada en la base de datos.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Ejecuta la operación dentro de una transacción. Mientras dura, ninguna otra operación
    /// con la misma <paramref name="lockKey"/> puede ejecutarse (bloqueo exclusivo a nivel de BD).
    /// Si la operación lanza una excepción, todos los cambios se revierten.
    /// </summary>
    Task<T> ExecuteLockedAsync<T>(string lockKey, Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
}
