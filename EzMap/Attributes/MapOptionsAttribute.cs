using System;

namespace EzMap;

/// <summary>
/// Configures mapping-specific options for a particular mapping pair.
/// Place this attribute on the mapper class alongside [Map&lt;TSource, TTarget&gt;].
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class MapOptionsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the generic type arguments this option applies to.
    /// Use Type objects for TSource and TTarget.
    /// </summary>
    public Type[]? ForMapping { get; set; }

    /// <summary>
    /// Gets or sets custom prefixes to remove during name normalization.
    /// Overrides global setting for this mapping.
    /// </summary>
    public string[]? CustomPrefixes { get; set; }

    /// <summary>
    /// Gets or sets custom suffixes to remove during name normalization.
    /// Overrides global setting for this mapping.
    /// </summary>
    public string[]? CustomSuffixes { get; set; }

    /// <summary>
    /// Gets or sets whether to enable prefix/suffix normalization.
    /// Overrides global setting for this mapping.
    /// </summary>
    public bool? EnablePrefixSuffixNormalization { get; set; }

    /// <summary>
    /// Gets or sets whether to use case-insensitive property matching.
    /// Overrides global setting for this mapping.
    /// </summary>
    public bool? CaseInsensitiveMatching { get; set; }

    /// <summary>
    /// Gets or sets how to handle nullable to non-nullable conversions.
    /// Overrides global setting for this mapping.
    /// </summary>
    public NullableFallbackBehavior? NullableFallbackBehavior { get; set; }

    /// <summary>
    /// Gets or sets whether to allow recursive mapping.
    /// Overrides global setting for this mapping.
    /// </summary>
    public bool? AllowRecursiveMapping { get; set; }

    /// <summary>
    /// Gets or sets the maximum recursion depth for recursive mappings.
    /// Overrides global setting for this mapping.
    /// </summary>
    public int? MaxRecursionDepth { get; set; }

    /// <summary>
    /// Gets or sets the constructor selection strategy.
    /// Overrides global setting for this mapping.
    /// </summary>
    public ConstructorSelectionStrategy? ConstructorSelectionStrategy { get; set; }

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
}
