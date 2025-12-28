namespace Blanquita.Domain.Exceptions;

public class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string entityName, string identifier)
        : base($"{entityName} with identifier '{identifier}' already exists")
    {
    }
}
