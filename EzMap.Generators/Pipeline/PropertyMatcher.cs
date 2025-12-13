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
        Location reportLocation,
        MappingOptions options)
    {
        var mappings = new List<PropertyMapping>();
        var diagnostics = new List<Diagnostic>();

        var sourceProperties = SymbolHelpers.GetReadableProperties(sourceType).ToList();
        var targetProperties = SymbolHelpers.GetWritableProperties(targetType).ToList();

        foreach (var sourceProp in sourceProperties)
        {
            // Check for explicit mapping first
            var explicitMapping = options.ExplicitMappings
                .FirstOrDefault(m => m.SourceProperty == sourceProp.Name);

            IPropertySymbol? targetProp = null;
            bool usedNormalization = false;

            if (!string.IsNullOrEmpty(explicitMapping.TargetProperty))
            {
                // Use explicit mapping
                targetProp = targetProperties.FirstOrDefault(p => p.Name == explicitMapping.TargetProperty);
            }
            else
            {
                // Use automatic matching
                var match = FindMatchingProperty(sourceProp, targetProperties, options);
                targetProp = match.targetProperty;
                usedNormalization = match.usedNormalization;
            }
            
            if (targetProp == null)
            {
                // No match found - emit warning
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.UnmatchedSourceProperty,
                    reportLocation,
                    sourceProp.Name));
                continue;
            }

            var (strategy, mapperMethodName) = DetermineStrategy(sourceProp, targetProp, compilation, options);
            var requiresNullHandling = NullabilityHelper.RequiresNullHandling(sourceProp.Type, targetProp.Type);

            // Emit info diagnostic if normalization was used
            if (usedNormalization)
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.UsingNormalizedNameMatch,
                    reportLocation,
                    sourceProp.Name,
                    targetProp.Name));
            }

            // Emit warning if nullable to non-nullable (based on config)
            if (requiresNullHandling && options.NullableFallbackBehavior == NullableFallbackBehavior.Diagnostic)
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
                    SymbolHelpers.GetMinimalName(targetProp.Type)));
            }

            // Emit info if using recursive mapping
            if (strategy == PropertyMappingStrategy.RecursiveMapping)
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.RecursiveMappingDetected,
                    reportLocation,
                    sourceProp.Name,
                    SymbolHelpers.GetMinimalName(sourceProp.Type),
                    SymbolHelpers.GetMinimalName(targetProp.Type)));
            }

            mappings.Add(new PropertyMapping(
                sourceProp.Name,
                targetProp.Name,
                SymbolHelpers.GetFullyQualifiedName(sourceProp.Type),
                SymbolHelpers.GetFullyQualifiedName(targetProp.Type),
                strategy,
                requiresNullHandling,
                usedNormalization,
                mapperMethodName));
        }

        return (mappings.ToImmutableArray(), diagnostics);
    }

    private static (IPropertySymbol? targetProperty, bool usedNormalization) FindMatchingProperty(
        IPropertySymbol sourceProperty,
        List<IPropertySymbol> targetProperties,
        MappingOptions options)
    {
        // 1. Exact name match (case-sensitive unless disabled)
        if (!options.CaseInsensitiveMatching)
        {
            var exactMatch = targetProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);
            if (exactMatch != null)
                return (exactMatch, false);
        }
        else
        {
            var caseInsensitiveMatch = targetProperties.FirstOrDefault(p => 
                string.Equals(p.Name, sourceProperty.Name, StringComparison.OrdinalIgnoreCase));
            if (caseInsensitiveMatch != null)
                return (caseInsensitiveMatch, false);
        }

        // 2. Normalized name match (if enabled)
        if (options.EnablePrefixSuffixNormalization)
        {
            foreach (var targetProp in targetProperties)
            {
                if (NameNormalizer.NormalizedEquals(
                    sourceProperty.Name, 
                    targetProp.Name, 
                    options.CustomPrefixes,
                    options.CustomSuffixes,
                    options.CaseInsensitiveMatching))
                {
                    return (targetProp, true);
                }
            }
        }

        return (null, false);
    }

    private static (PropertyMappingStrategy strategy, string? mapperMethodName) DetermineStrategy(
        IPropertySymbol sourceProperty,
        IPropertySymbol targetProperty,
        Compilation compilation,
        MappingOptions options)
    {
        var sourceType = sourceProperty.Type;
        var targetType = targetProperty.Type;

        // Get underlying types if nullable
        var sourceUnderlying = NullabilityHelper.GetUnderlyingType(sourceType);
        var targetUnderlying = NullabilityHelper.GetUnderlyingType(targetType);

        // Direct assignment if same underlying type
        if (SymbolEqualityComparer.Default.Equals(sourceUnderlying, targetUnderlying))
            return (PropertyMappingStrategy.DirectAssignment, null);

        // Check for conversions if we have a C# compilation
        if (compilation is CSharpCompilation csharpCompilation)
        {
            var conversion = csharpCompilation.ClassifyConversion(sourceType, targetType);
            if (conversion.IsImplicit)
                return (PropertyMappingStrategy.ImplicitCast, null);

            if (conversion.IsExplicit)
                return (PropertyMappingStrategy.ExplicitCast, null);
        }

        // Check for recursive mapping if enabled
        if (options.AllowRecursiveMapping)
        {
            // Check if both types are reference types (classes) that could have mappers
            // and they are DIFFERENT types
            if (!SymbolEqualityComparer.Default.Equals(sourceUnderlying, targetUnderlying) &&
                sourceUnderlying.TypeKind == TypeKind.Class && 
                targetUnderlying.TypeKind == TypeKind.Class &&
                sourceUnderlying.SpecialType == SpecialType.None &&
                targetUnderlying.SpecialType == SpecialType.None)
            {
                // Generate mapper method name based on target type
                var targetTypeName = SymbolHelpers.GetSimpleName(targetUnderlying);
                var mapperMethodName = $"MapTo{targetTypeName}";
                
                return (PropertyMappingStrategy.RecursiveMapping, mapperMethodName);
            }
        }

        return (PropertyMappingStrategy.UseDefault, null);
    }
}
