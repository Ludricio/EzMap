namespace EzMap;

/// <summary>
/// Specifies that mapping methods should be generated for the two types.
/// Place this attribute on a static partial class to generate mapping extension methods.
/// </summary>
/// <typeparam name="TSource">The first type to map.</typeparam>
/// <typeparam name="TTarget">The second type to map.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class MapAttribute<TSource, TTarget> : Attribute
{
    /// <summary>
    /// Gets or sets whether to generate instance extension methods (e.g., source.MapToTarget()).
    /// Default is true.
    /// </summary>
    public bool GenerateInstanceExtensions { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate static extension members using new C# extension syntax (e.g., TSource.From(target)).
    /// Default is false (feature detection applies).
    /// </summary>
    public bool GenerateStaticExtensions { get; set; } = false;

    /// <summary>
    /// Gets or sets the mapping direction.
    /// Default is MappingDirection.Both.
    /// </summary>
    public MappingDirection MappingDirection { get; set; } = MappingDirection.Both;

    /// <summary>
    /// Gets or sets whether to generate mapping hooks (BeforeMap/AfterMap methods).
    /// Default is false.
    /// </summary>
    public bool GenerateMappingHooks { get; set; } = false;

    /// <summary>
    /// Gets or sets custom prefixes to remove during name normalization for this mapping.
    /// If null, uses global or default configuration.
    /// </summary>
    public string[]? CustomPrefixes { get; set; }

    /// <summary>
    /// Gets or sets custom suffixes to remove during name normalization for this mapping.
    /// If null, uses global or default configuration.
    /// </summary>
    public string[]? CustomSuffixes { get; set; }

    /// <summary>
    /// Gets or sets whether to enable prefix/suffix normalization for this mapping.
    /// If null, uses global configuration.
    /// </summary>
    public bool? EnablePrefixSuffixNormalization { get; set; }

    /// <summary>
    /// Gets or sets whether to use case-insensitive property matching for this mapping.
    /// If null, uses global configuration.
    /// </summary>
    public bool? CaseInsensitiveMatching { get; set; }

    /// <summary>
    /// Gets or sets how to handle nullable to non-nullable conversions for this mapping.
    /// If null, uses global configuration.
    /// </summary>
    public NullableFallbackBehavior? NullableFallbackBehavior { get; set; }

    /// <summary>
    /// Gets or sets whether to allow recursive mapping for this mapping.
    /// If null, uses global configuration.
    /// </summary>
    public bool? AllowRecursiveMapping { get; set; }

    /// <summary>
    /// Gets or sets the maximum recursion depth for recursive mappings.
    /// If null, uses global configuration.
    /// </summary>
    public int? MaxRecursionDepth { get; set; }

    /// <summary>
    /// Gets or sets the constructor selection strategy for this mapping.
    /// If null, uses global configuration.
    /// </summary>
    public ConstructorSelectionStrategy? ConstructorSelectionStrategy { get; set; }
}
