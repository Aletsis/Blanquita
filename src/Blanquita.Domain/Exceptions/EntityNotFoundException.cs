namespace Blanquita.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} with ID {id} was not found")
    {
    }

    public EntityNotFoundException(string entityName, string identifier)
        : base($"{entityName} with identifier '{identifier}' was not found")
    {
    }
}
