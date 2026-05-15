using System.Reflection;

namespace SharedKernel.Domain;

public static class DomainExtensions
{
    extension(BaseEntity entity)
    {
        public string ToStringReflection()
        {
            return entity.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .Select(p => $"{p.Name}: {p.GetValue(entity)}")
                .Aggregate((current, next) => $"{current}, {next}");
        }
    }
}