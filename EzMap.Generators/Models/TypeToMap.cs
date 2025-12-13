using Microsoft.CodeAnalysis;
using System;

namespace EzMap.Generators.Models;

/// <summary>
/// Represents a type that should be included in mapping.
/// </summary>
internal readonly struct TypeToMap : IEquatable<TypeToMap>
{
    public TypeToMap(string fullyQualifiedName, string simpleName, bool isNullable)
    {
        FullyQualifiedName = fullyQualifiedName;
        SimpleName = simpleName;
        IsNullable = isNullable;
    }

    /// <summary>
    /// The fully qualified name of the type (e.g., "global::MyNamespace.MyType").
    /// </summary>
    public string FullyQualifiedName { get; }

    /// <summary>
    /// The simple name of the type (e.g., "MyType").
    /// </summary>
    public string SimpleName { get; }

    /// <summary>
    /// Whether this type is nullable.
    /// </summary>
    public bool IsNullable { get; }

    public bool Equals(TypeToMap other)
    {
        return FullyQualifiedName == other.FullyQualifiedName &&
               SimpleName == other.SimpleName &&
               IsNullable == other.IsNullable;
    }

    public override bool Equals(object? obj)
    {
        return obj is TypeToMap other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + (FullyQualifiedName?.GetHashCode() ?? 0);
            hash = hash * 31 + (SimpleName?.GetHashCode() ?? 0);
            hash = hash * 31 + IsNullable.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(TypeToMap left, TypeToMap right) => left.Equals(right);
    public static bool operator !=(TypeToMap left, TypeToMap right) => !left.Equals(right);
}
