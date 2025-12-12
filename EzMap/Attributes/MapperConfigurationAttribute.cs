using System;

namespace EzMap;

/// <summary>
/// Configures global mapping options for the assembly.
/// Place this attribute at the assembly level.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class MapperConfigurationAttribute : Attribute
{
    /// <summary>
    /// Gets or sets custom prefixes to remove during name normalization.
    /// If null, uses default underscore prefix.
    /// </summary>
    public string[]? CustomPrefixes { get; set; }

    /// <summary>
    /// Gets or sets custom suffixes to remove during name normalization.
    /// If null, uses default suffixes (Dto, Entity, Model, ViewModel).
    /// </summary>
    public string[]? CustomSuffixes { get; set; }

    /// <summary>
    /// Gets or sets whether to enable prefix/suffix normalization.
    /// Default is true.
    /// </summary>
    public bool EnablePrefixSuffixNormalization { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use case-insensitive property matching.
    /// Default is true.
    /// </summary>
    public bool CaseInsensitiveMatching { get; set; } = true;

    /// <summary>
    /// Gets or sets how to handle nullable to non-nullable conversions.
    /// Default is NullableFallbackBehavior.Default (use default(T)).
    /// </summary>
    public NullableFallbackBehavior NullableFallbackBehavior { get; set; } = NullableFallbackBehavior.Default;

    /// <summary>
    /// Gets or sets whether to allow recursive mapping.
    /// Default is false.
    /// </summary>
    public bool AllowRecursiveMapping { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum recursion depth for recursive mappings.
    /// Default is 5.
    /// </summary>
    public int MaxRecursionDepth { get; set; } = 5;

    /// <summary>
    /// Gets or sets the default constructor selection strategy.
    /// Default is ConstructorSelectionStrategy.Parameterless.
    /// </summary>
    public ConstructorSelectionStrategy ConstructorSelectionStrategy { get; set; } = ConstructorSelectionStrategy.Parameterless;
}
