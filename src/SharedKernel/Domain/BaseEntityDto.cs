namespace SharedKernel.Domain;

public abstract record BaseEntityDto(Guid Id, DateTime CreatedOnUtc, DateTime? UpdatedOnUtc);
