using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;

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
    /// Gets a simple name for a type symbol (without namespace or generics).
    /// </summary>
    public static string GetSimpleName(ITypeSymbol typeSymbol)
    {
        // For generic types, get the name without type parameters
        if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            return namedType.Name;
        }
        
        return typeSymbol.Name;
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
    /// Gets all instance properties that are readable from a type, including inherited properties.
    /// </summary>
    public static IEnumerable<IPropertySymbol> GetReadableProperties(ITypeSymbol typeSymbol)
    {
        var currentType = typeSymbol;
        var seen = new HashSet<string>();

        while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in currentType.GetMembers())
            {
                if (member is IPropertySymbol property && 
                    !property.IsStatic && 
                    property.GetMethod != null &&
                    property.GetMethod.DeclaredAccessibility == Accessibility.Public &&
                    !seen.Contains(property.Name))
                {
                    seen.Add(property.Name);
                    yield return property;
                }
            }

            currentType = currentType.BaseType;
        }
    }

    /// <summary>
    /// Gets all instance properties that are writable to a type, including inherited properties.
    /// </summary>
    public static IEnumerable<IPropertySymbol> GetWritableProperties(ITypeSymbol typeSymbol)
    {
        var currentType = typeSymbol;
        var seen = new HashSet<string>();

        while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in currentType.GetMembers())
            {
                if (member is IPropertySymbol property && 
                    !property.IsStatic &&
                    !seen.Contains(property.Name))
                {
                    // Check if property has a public setter
                    if (property.SetMethod != null && property.SetMethod.DeclaredAccessibility == Accessibility.Public)
                    {
                        seen.Add(property.Name);
                        yield return property;
                    }
                    // Or if it's init-only with public accessibility
                    else if (property.SetMethod?.IsInitOnly == true && property.DeclaredAccessibility == Accessibility.Public)
                    {
                        seen.Add(property.Name);
                        yield return property;
                    }
                }
            }

            currentType = currentType.BaseType;
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

    /// <summary>
    /// Gets the best constructor for the given strategy.
    /// </summary>
    public static IMethodSymbol? GetBestConstructor(
        ITypeSymbol typeSymbol, 
        ConstructorSelectionStrategy strategy,
        ISymbol accessibleFrom)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
            return null;

        var constructors = namedType.Constructors
            .Where(c => !c.IsStatic && IsAccessible(c, accessibleFrom))
            .ToList();

        if (!constructors.Any())
            return null;

        switch (strategy)
        {
            case ConstructorSelectionStrategy.Parameterless:
                return constructors.FirstOrDefault(c => c.Parameters.Length == 0);

            case ConstructorSelectionStrategy.Greediest:
                return constructors.OrderByDescending(c => c.Parameters.Length).FirstOrDefault();

            case ConstructorSelectionStrategy.Annotated:
                // Look for constructor with [MapConstructor] attribute
                var annotatedCtor = constructors.FirstOrDefault(c =>
                    c.GetAttributes().Any(a => 
                        a.AttributeClass?.Name == "MapConstructorAttribute" ||
                        a.AttributeClass?.Name == "MapConstructor"));
                
                // Fallback to parameterless if no annotation found
                return annotatedCtor ?? constructors.FirstOrDefault(c => c.Parameters.Length == 0);

            default:
                return constructors.FirstOrDefault(c => c.Parameters.Length == 0);
        }
    }

    /// <summary>
    /// Checks if a member is accessible from a given context.
    /// </summary>
    private static bool IsAccessible(ISymbol symbol, ISymbol accessibleFrom)
    {
        var accessibility = symbol.DeclaredAccessibility;
        
        if (accessibility == Accessibility.Public)
            return true;

        if (accessibility == Accessibility.Internal &&
            SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, accessibleFrom.ContainingAssembly))
            return true;

        return false;
    }
}

