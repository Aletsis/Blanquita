namespace Blanquita.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
