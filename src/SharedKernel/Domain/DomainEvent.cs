namespace SharedKernel.Domain;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

public abstract record DomainEvent(DateTime OccurredOn) : IDomainEvent;
