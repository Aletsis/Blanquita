namespace Blanquita.Domain.Exceptions;

/// <summary>
/// Otro usuario guardó cambios sobre el mismo registro después de que se cargó para editar.
/// </summary>
public class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(string message) : base(message)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
