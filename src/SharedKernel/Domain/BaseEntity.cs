namespace SharedKernel.Domain;

public abstract class BaseEntity
{
    public override string ToString()
    {
        return this.ToStringReflection();
    }
}