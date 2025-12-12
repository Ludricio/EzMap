using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Utils;

/// <summary>
/// Provides helper methods for working with nullability information.
/// </summary>
internal static class NullabilityHelper
{
    /// <summary>
    /// Checks if a type symbol represents a non-nullable reference type or value type.
    /// </summary>
    public static bool IsNonNullable(ITypeSymbol typeSymbol)
    {
        // Value types (except Nullable<T>) are non-nullable
        if (typeSymbol.IsValueType)
        {
            if (typeSymbol is INamedTypeSymbol namedType &&
                namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                return false; // Nullable<T> is nullable
            }
            return true;
        }

        // Reference types with NotAnnotated are non-nullable in nullable context
        return typeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated;
    }

    /// <summary>
    /// Checks if a type symbol represents a nullable type.
    /// </summary>
    public static bool IsNullable(ITypeSymbol typeSymbol)
    {
        return !IsNonNullable(typeSymbol);
    }

    /// <summary>
    /// Gets the underlying non-nullable type from a nullable type.
    /// </summary>
    public static ITypeSymbol GetUnderlyingType(ITypeSymbol typeSymbol)
    {
        // For Nullable<T>, return T
        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedType.TypeArguments[0];
        }

        // For nullable reference types, return the non-nullable version
        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        }

        return typeSymbol;
    }

    /// <summary>
    /// Checks if mapping from source to target requires null handling.
    /// </summary>
    public static bool RequiresNullHandling(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        bool sourceIsNullable = IsNullable(sourceType);
        bool targetIsNonNullable = IsNonNullable(targetType);
        
        return sourceIsNullable && targetIsNonNullable;
    }

    /// <summary>
    /// Gets the default value expression for a type.
    /// </summary>
    public static string GetDefaultValueExpression(ITypeSymbol typeSymbol)
    {
        // For value types, return their appropriate default
        if (typeSymbol.IsValueType)
        {
            if (typeSymbol.SpecialType == SpecialType.System_Boolean)
                return "false";
            if (IsNumericType(typeSymbol))
                return "0";
            
            return $"default({SymbolHelpers.GetFullyQualifiedName(typeSymbol)})";
        }

        // For reference types, return default (which is null)
        return $"default({SymbolHelpers.GetFullyQualifiedName(typeSymbol)})";
    }

    private static bool IsNumericType(ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_Byte => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_Decimal => true,
            _ => false
        };
    }
}
