using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EzMap.Generators.Utils;

/// <summary>
/// Provides helper methods for working with Roslyn symbols.
/// </summary>
internal static class SymbolHelpers
{
    /// <summary>
    /// Gets the fully qualified name of a type symbol.
    /// </summary>
    public static string GetFullyQualifiedName(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    /// <summary>
    /// Gets a minimal display name for a type symbol.
    /// </summary>
    public static string GetMinimalName(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    }

    /// <summary>
    /// Checks if a type has an accessible parameterless constructor.
    /// </summary>
    public static bool HasAccessibleParameterlessConstructor(ITypeSymbol typeSymbol, ISymbol accessibleFrom)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
            return false;

        // Check for implicit parameterless constructor (no constructors defined)
        if (!namedType.Constructors.Any(c => !c.IsStatic))
            return true;

        // Check for explicit parameterless constructor
        foreach (var constructor in namedType.Constructors)
        {
            if (constructor.IsStatic)
                continue;

            if (constructor.Parameters.Length == 0)
            {
                // Check accessibility
                var accessibility = constructor.DeclaredAccessibility;
                if (accessibility == Accessibility.Public)
                    return true;

                if (accessibility == Accessibility.Internal && 
                    SymbolEqualityComparer.Default.Equals(constructor.ContainingAssembly, accessibleFrom.ContainingAssembly))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all instance properties that are readable from a type.
    /// </summary>
    public static IEnumerable<IPropertySymbol> GetReadableProperties(ITypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IPropertySymbol property && 
                !property.IsStatic && 
                property.GetMethod != null &&
                property.GetMethod.DeclaredAccessibility == Accessibility.Public)
            {
                yield return property;
            }
        }
    }

    /// <summary>
    /// Gets all instance properties that are writable to a type.
    /// </summary>
    public static IEnumerable<IPropertySymbol> GetWritableProperties(ITypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IPropertySymbol property && 
                !property.IsStatic && 
                (property.SetMethod != null || property.IsInitOnly()) &&
                (property.SetMethod?.DeclaredAccessibility == Accessibility.Public || property.IsInitOnly()))
            {
                yield return property;
            }
        }
    }

    /// <summary>
    /// Checks if a property is init-only.
    /// </summary>
    private static bool IsInitOnly(this IPropertySymbol property)
    {
        return property.SetMethod?.IsInitOnly == true;
    }

    /// <summary>
    /// Determines if a type is nullable.
    /// </summary>
    public static bool IsNullable(ITypeSymbol typeSymbol, out ITypeSymbol underlyingType)
    {
        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            underlyingType = typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            return true;
        }

        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            underlyingType = namedType.TypeArguments[0];
            return true;
        }

        underlyingType = typeSymbol;
        return false;
    }

    /// <summary>
    /// Checks if one type can be assigned to another.
    /// </summary>
    public static bool CanAssign(ITypeSymbol targetType, ITypeSymbol sourceType, Compilation compilation)
    {
        // Direct assignment
        if (SymbolEqualityComparer.Default.Equals(targetType, sourceType))
            return true;

        // Check for conversion
        if (compilation is CSharpCompilation csharpCompilation)
        {
            var conversion = csharpCompilation.ClassifyConversion(sourceType, targetType);
            return conversion.IsImplicit || conversion.IsExplicit;
        }

        return false;
    }
}
