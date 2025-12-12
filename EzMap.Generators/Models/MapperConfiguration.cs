using Microsoft.CodeAnalysis;
using System;

namespace EzMap.Generators.Models;

/// <summary>
/// Represents the configuration for a mapper class.
/// </summary>
internal readonly struct MapperConfiguration : IEquatable<MapperConfiguration>
{
    public MapperConfiguration(
        string className,
        string classNamespace,
        TypeToMap sourceType,
        TypeToMap targetType,
        bool generateInstanceExtensions,
        bool generateStaticExtensions,
        Location attributeLocation)
    {
        ClassName = className;
        ClassNamespace = classNamespace;
        SourceType = sourceType;
        TargetType = targetType;
        GenerateInstanceExtensions = generateInstanceExtensions;
        GenerateStaticExtensions = generateStaticExtensions;
        AttributeLocation = attributeLocation;
    }

    /// <summary>
    /// The name of the static partial class containing the mapping methods.
    /// </summary>
    public string ClassName { get; }

    /// <summary>
    /// The namespace of the static partial class.
    /// </summary>
    public string ClassNamespace { get; }

    /// <summary>
    /// The source type in the mapping.
    /// </summary>
    public TypeToMap SourceType { get; }

    /// <summary>
    /// The target type in the mapping.
    /// </summary>
    public TypeToMap TargetType { get; }

    /// <summary>
    /// Whether to generate instance extension methods.
    /// </summary>
    public bool GenerateInstanceExtensions { get; }

    /// <summary>
    /// Whether to generate static extension members (new C# syntax).
    /// </summary>
    public bool GenerateStaticExtensions { get; }

    /// <summary>
    /// The location of the Map attribute for diagnostic reporting.
    /// </summary>
    public Location AttributeLocation { get; }

    public bool Equals(MapperConfiguration other)
    {
        return ClassName == other.ClassName &&
               ClassNamespace == other.ClassNamespace &&
               SourceType.Equals(other.SourceType) &&
               TargetType.Equals(other.TargetType) &&
               GenerateInstanceExtensions == other.GenerateInstanceExtensions &&
               GenerateStaticExtensions == other.GenerateStaticExtensions;
    }

    public override bool Equals(object? obj)
    {
        return obj is MapperConfiguration other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + (ClassName?.GetHashCode() ?? 0);
            hash = hash * 31 + (ClassNamespace?.GetHashCode() ?? 0);
            hash = hash * 31 + SourceType.GetHashCode();
            hash = hash * 31 + TargetType.GetHashCode();
            hash = hash * 31 + GenerateInstanceExtensions.GetHashCode();
            hash = hash * 31 + GenerateStaticExtensions.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(MapperConfiguration left, MapperConfiguration right) => left.Equals(right);
    public static bool operator !=(MapperConfiguration left, MapperConfiguration right) => !left.Equals(right);
}
