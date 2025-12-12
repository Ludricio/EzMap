using System;
using System.Collections.Immutable;

namespace EzMap.Generators.Models;

/// <summary>
/// Represents the resolved mapping configuration combining global and per-mapping settings.
/// </summary>
internal readonly struct MappingOptions : IEquatable<MappingOptions>
{
    public MappingOptions(
        ImmutableArray<string> customPrefixes,
        ImmutableArray<string> customSuffixes,
        bool enablePrefixSuffixNormalization,
        bool caseInsensitiveMatching,
        NullableFallbackBehavior nullableFallbackBehavior,
        bool allowRecursiveMapping,
        int maxRecursionDepth,
        ConstructorSelectionStrategy constructorSelectionStrategy,
        MappingDirection mappingDirection,
        bool generateMappingHooks,
        ImmutableArray<ExplicitPropertyMapping> explicitMappings)
    {
        CustomPrefixes = customPrefixes;
        CustomSuffixes = customSuffixes;
        EnablePrefixSuffixNormalization = enablePrefixSuffixNormalization;
        CaseInsensitiveMatching = caseInsensitiveMatching;
        NullableFallbackBehavior = nullableFallbackBehavior;
        AllowRecursiveMapping = allowRecursiveMapping;
        MaxRecursionDepth = maxRecursionDepth;
        ConstructorSelectionStrategy = constructorSelectionStrategy;
        MappingDirection = mappingDirection;
        GenerateMappingHooks = generateMappingHooks;
        ExplicitMappings = explicitMappings;
    }

    public ImmutableArray<string> CustomPrefixes { get; }
    public ImmutableArray<string> CustomSuffixes { get; }
    public bool EnablePrefixSuffixNormalization { get; }
    public bool CaseInsensitiveMatching { get; }
    public NullableFallbackBehavior NullableFallbackBehavior { get; }
    public bool AllowRecursiveMapping { get; }
    public int MaxRecursionDepth { get; }
    public ConstructorSelectionStrategy ConstructorSelectionStrategy { get; }
    public MappingDirection MappingDirection { get; }
    public bool GenerateMappingHooks { get; }
    public ImmutableArray<ExplicitPropertyMapping> ExplicitMappings { get; }

    public bool Equals(MappingOptions other)
    {
        return CustomPrefixes.SequenceEqual(other.CustomPrefixes) &&
               CustomSuffixes.SequenceEqual(other.CustomSuffixes) &&
               EnablePrefixSuffixNormalization == other.EnablePrefixSuffixNormalization &&
               CaseInsensitiveMatching == other.CaseInsensitiveMatching &&
               NullableFallbackBehavior == other.NullableFallbackBehavior &&
               AllowRecursiveMapping == other.AllowRecursiveMapping &&
               MaxRecursionDepth == other.MaxRecursionDepth &&
               ConstructorSelectionStrategy == other.ConstructorSelectionStrategy &&
               MappingDirection == other.MappingDirection &&
               GenerateMappingHooks == other.GenerateMappingHooks &&
               ExplicitMappings.SequenceEqual(other.ExplicitMappings);
    }

    public override bool Equals(object? obj)
    {
        return obj is MappingOptions other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            foreach (var prefix in CustomPrefixes)
                hash = hash * 31 + (prefix?.GetHashCode() ?? 0);
            foreach (var suffix in CustomSuffixes)
                hash = hash * 31 + (suffix?.GetHashCode() ?? 0);
            hash = hash * 31 + EnablePrefixSuffixNormalization.GetHashCode();
            hash = hash * 31 + CaseInsensitiveMatching.GetHashCode();
            hash = hash * 31 + NullableFallbackBehavior.GetHashCode();
            hash = hash * 31 + AllowRecursiveMapping.GetHashCode();
            hash = hash * 31 + MaxRecursionDepth.GetHashCode();
            hash = hash * 31 + ConstructorSelectionStrategy.GetHashCode();
            hash = hash * 31 + MappingDirection.GetHashCode();
            hash = hash * 31 + GenerateMappingHooks.GetHashCode();
            foreach (var mapping in ExplicitMappings)
                hash = hash * 31 + mapping.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(MappingOptions left, MappingOptions right) => left.Equals(right);
    public static bool operator !=(MappingOptions left, MappingOptions right) => !left.Equals(right);
}

/// <summary>
/// Represents an explicit property mapping configuration.
/// </summary>
internal readonly struct ExplicitPropertyMapping : IEquatable<ExplicitPropertyMapping>
{
    public ExplicitPropertyMapping(string sourceProperty, string targetProperty)
    {
        SourceProperty = sourceProperty;
        TargetProperty = targetProperty;
    }

    public string SourceProperty { get; }
    public string TargetProperty { get; }

    public bool Equals(ExplicitPropertyMapping other)
    {
        return SourceProperty == other.SourceProperty &&
               TargetProperty == other.TargetProperty;
    }

    public override bool Equals(object? obj)
    {
        return obj is ExplicitPropertyMapping other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + (SourceProperty?.GetHashCode() ?? 0);
            hash = hash * 31 + (TargetProperty?.GetHashCode() ?? 0);
            return hash;
        }
    }

    public static bool operator ==(ExplicitPropertyMapping left, ExplicitPropertyMapping right) => left.Equals(right);
    public static bool operator !=(ExplicitPropertyMapping left, ExplicitPropertyMapping right) => !left.Equals(right);
}
