using System;

namespace EzMap.Generators.Models;

/// <summary>
/// Defines how a property should be mapped from source to target.
/// </summary>
internal readonly struct PropertyMapping : IEquatable<PropertyMapping>
{
    public PropertyMapping(
        string sourcePropertyName,
        string targetPropertyName,
        string sourcePropertyType,
        string targetPropertyType,
        PropertyMappingStrategy strategy,
        bool requiresNullHandling,
        bool usedNormalization)
    {
        SourcePropertyName = sourcePropertyName;
        TargetPropertyName = targetPropertyName;
        SourcePropertyType = sourcePropertyType;
        TargetPropertyType = targetPropertyType;
        Strategy = strategy;
        RequiresNullHandling = requiresNullHandling;
        UsedNormalization = usedNormalization;
    }

    /// <summary>
    /// The name of the property in the source type.
    /// </summary>
    public string SourcePropertyName { get; }

    /// <summary>
    /// The name of the property in the target type.
    /// </summary>
    public string TargetPropertyName { get; }

    /// <summary>
    /// The fully qualified type name of the source property.
    /// </summary>
    public string SourcePropertyType { get; }

    /// <summary>
    /// The fully qualified type name of the target property.
    /// </summary>
    public string TargetPropertyType { get; }

    /// <summary>
    /// The strategy to use for mapping this property.
    /// </summary>
    public PropertyMappingStrategy Strategy { get; }

    /// <summary>
    /// Whether this mapping requires null handling (nullable to non-nullable).
    /// </summary>
    public bool RequiresNullHandling { get; }

    /// <summary>
    /// Whether name normalization was used to match this property.
    /// </summary>
    public bool UsedNormalization { get; }

    public bool Equals(PropertyMapping other)
    {
        return SourcePropertyName == other.SourcePropertyName &&
               TargetPropertyName == other.TargetPropertyName &&
               SourcePropertyType == other.SourcePropertyType &&
               TargetPropertyType == other.TargetPropertyType &&
               Strategy == other.Strategy &&
               RequiresNullHandling == other.RequiresNullHandling &&
               UsedNormalization == other.UsedNormalization;
    }

    public override bool Equals(object? obj)
    {
        return obj is PropertyMapping other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + (SourcePropertyName?.GetHashCode() ?? 0);
            hash = hash * 31 + (TargetPropertyName?.GetHashCode() ?? 0);
            hash = hash * 31 + (SourcePropertyType?.GetHashCode() ?? 0);
            hash = hash * 31 + (TargetPropertyType?.GetHashCode() ?? 0);
            hash = hash * 31 + Strategy.GetHashCode();
            hash = hash * 31 + RequiresNullHandling.GetHashCode();
            hash = hash * 31 + UsedNormalization.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(PropertyMapping left, PropertyMapping right) => left.Equals(right);
    public static bool operator !=(PropertyMapping left, PropertyMapping right) => !left.Equals(right);
}

/// <summary>
/// Defines the strategy for mapping a property value.
/// </summary>
internal enum PropertyMappingStrategy
{
    /// <summary>
    /// Direct assignment (same type).
    /// </summary>
    DirectAssignment,

    /// <summary>
    /// Implicit cast.
    /// </summary>
    ImplicitCast,

    /// <summary>
    /// Explicit cast.
    /// </summary>
    ExplicitCast,

    /// <summary>
    /// Recursive mapping (call another mapping method).
    /// </summary>
    RecursiveMapping,

    /// <summary>
    /// Use default value (for unmappable or null source).
    /// </summary>
    UseDefault
}
