using Microsoft.Extensions.Compliance.Classification;

namespace SharedKernel.Compliance;

/// <summary>
/// Attribute to mark a parameter, field, or property as End User Identifiable Information (EUII).
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class EuiiDataAttribute : DataClassificationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EuiiDataAttribute"/> class.
    /// </summary>
    public EuiiDataAttribute()
        : base(LoggingTaxonomyDefinitions.EUIIDataClassification) { }
    /// <summary>
    /// Attribute to mark a parameter, field, or property as End User Identifiable Information (EUII).
    /// </summary>
}