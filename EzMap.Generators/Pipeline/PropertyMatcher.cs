using EzMap.Generators.Diagnostics;
using EzMap.Generators.Models;
using EzMap.Generators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace EzMap.Generators.Pipeline;

/// <summary>
/// Matches properties between source and target types and determines mapping strategies.
/// </summary>
internal static class PropertyMatcher
{
    /// <summary>
    /// Matches properties from source to target type.
    /// </summary>
    public static (ImmutableArray<PropertyMapping> mappings, List<Diagnostic> diagnostics) MatchProperties(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        Compilation compilation,
        Location reportLocation)
    {
        var mappings = new List<PropertyMapping>();
        var diagnostics = new List<Diagnostic>();

        var sourceProperties = SymbolHelpers.GetReadableProperties(sourceType).ToList();
        var targetProperties = SymbolHelpers.GetWritableProperties(targetType).ToList();

        foreach (var sourceProp in sourceProperties)
        {
            var match = FindMatchingProperty(sourceProp, targetProperties);
            
            if (match.targetProperty == null)
            {
                // No match found - emit warning
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.UnmatchedSourceProperty,
                    reportLocation,
                    sourceProp.Name));
                continue;
            }

            var strategy = DetermineStrategy(sourceProp, match.targetProperty, compilation);
            var requiresNullHandling = NullabilityHelper.RequiresNullHandling(sourceProp.Type, match.targetProperty.Type);

            // Emit info diagnostic if normalization was used
            if (match.usedNormalization)
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.UsingNormalizedNameMatch,
                    reportLocation,
                    sourceProp.Name,
                    match.targetProperty.Name));
            }

            // Emit warning if nullable to non-nullable
            if (requiresNullHandling)
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.NullableToNonNullableMapping,
                    reportLocation,
                    sourceProp.Name));
            }

            // Emit info if using explicit cast
            if (strategy == PropertyMappingStrategy.ExplicitCast)
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.UsingExplicitCast,
                    reportLocation,
                    sourceProp.Name,
                    SymbolHelpers.GetMinimalName(sourceProp.Type),
                    SymbolHelpers.GetMinimalName(match.targetProperty.Type)));
            }

            mappings.Add(new PropertyMapping(
                sourceProp.Name,
                match.targetProperty.Name,
                SymbolHelpers.GetFullyQualifiedName(sourceProp.Type),
                SymbolHelpers.GetFullyQualifiedName(match.targetProperty.Type),
                strategy,
                requiresNullHandling,
                match.usedNormalization));
        }

        return (mappings.ToImmutableArray(), diagnostics);
    }

    private static (IPropertySymbol? targetProperty, bool usedNormalization) FindMatchingProperty(
        IPropertySymbol sourceProperty,
        List<IPropertySymbol> targetProperties)
    {
        // 1. Exact name match (case-sensitive)
        var exactMatch = targetProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);
        if (exactMatch != null)
            return (exactMatch, false);

        // 2. Normalized name match
        foreach (var targetProp in targetProperties)
        {
            if (NameNormalizer.NormalizedEquals(sourceProperty.Name, targetProp.Name))
                return (targetProp, true);
        }

        return (null, false);
    }

    private static PropertyMappingStrategy DetermineStrategy(
        IPropertySymbol sourceProperty,
        IPropertySymbol targetProperty,
        Compilation compilation)
    {
        var sourceType = sourceProperty.Type;
        var targetType = targetProperty.Type;

        // Get underlying types if nullable
        var sourceUnderlying = NullabilityHelper.GetUnderlyingType(sourceType);
        var targetUnderlying = NullabilityHelper.GetUnderlyingType(targetType);

        // Direct assignment if same underlying type
        if (SymbolEqualityComparer.Default.Equals(sourceUnderlying, targetUnderlying))
            return PropertyMappingStrategy.DirectAssignment;

        // Check for conversions if we have a C# compilation
        if (compilation is CSharpCompilation csharpCompilation)
        {
            var conversion = csharpCompilation.ClassifyConversion(sourceType, targetType);
            if (conversion.IsImplicit)
                return PropertyMappingStrategy.ImplicitCast;

            if (conversion.IsExplicit)
                return PropertyMappingStrategy.ExplicitCast;
        }

        // For now, use default - recursive mapping would be more complex
        return PropertyMappingStrategy.UseDefault;
    }
}
