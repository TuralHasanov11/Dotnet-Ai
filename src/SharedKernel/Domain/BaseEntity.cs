using System.ComponentModel.DataAnnotations.Schema;

namespace SharedKernel.Domain;

public abstract class BaseEntity : IAggregateRoot
{
    public Guid Id { get; }

    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; protected set; }

    public byte[]? RowVersion { get; }
    private readonly List<DomainEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void AddDomainEvent(DomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    protected void RemoveDomainEvent(DomainEvent eventItem)
    {
        _domainEvents?.Remove(eventItem);
    }

    public override string ToString()
    {
        return this.ToStringReflection();
    }
}