using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace EzMap.Generators.Models;

/// <summary>
/// Contains all the information needed to generate mapping code.
/// </summary>
internal readonly struct GenerationContext : IEquatable<GenerationContext>
{
    public GenerationContext(
        MapperConfiguration configuration,
        ImmutableArray<PropertyMapping> sourceToTargetMappings,
        ImmutableArray<PropertyMapping> targetToSourceMappings,
        ImmutableArray<Diagnostic> diagnostics)
    {
        Configuration = configuration;
        SourceToTargetMappings = sourceToTargetMappings;
        TargetToSourceMappings = targetToSourceMappings;
        Diagnostics = diagnostics;
    }

    /// <summary>
    /// The mapper configuration.
    /// </summary>
    public MapperConfiguration Configuration { get; }

    /// <summary>
    /// Property mappings from source to target.
    /// </summary>
    public ImmutableArray<PropertyMapping> SourceToTargetMappings { get; }

    /// <summary>
    /// Property mappings from target to source.
    /// </summary>
    public ImmutableArray<PropertyMapping> TargetToSourceMappings { get; }

    /// <summary>
    /// Diagnostics to report.
    /// </summary>
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public bool Equals(GenerationContext other)
    {
        return Configuration.Equals(other.Configuration) &&
               SourceToTargetMappings.SequenceEqual(other.SourceToTargetMappings) &&
               TargetToSourceMappings.SequenceEqual(other.TargetToSourceMappings);
    }

    public override bool Equals(object? obj)
    {
        return obj is GenerationContext other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + Configuration.GetHashCode();
            
            foreach (var mapping in SourceToTargetMappings)
            {
                hash = hash * 31 + mapping.GetHashCode();
            }
            
            foreach (var mapping in TargetToSourceMappings)
            {
                hash = hash * 31 + mapping.GetHashCode();
            }
            
            return hash;
        }
    }

    public static bool operator ==(GenerationContext left, GenerationContext right) => left.Equals(right);
    public static bool operator !=(GenerationContext left, GenerationContext right) => !left.Equals(right);
}
