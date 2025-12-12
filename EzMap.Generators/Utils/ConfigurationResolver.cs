using EzMap.Generators.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace EzMap.Generators.Utils;

/// <summary>
/// Resolves mapping configuration from global and per-mapping attributes.
/// </summary>
internal static class ConfigurationResolver
{
    private static readonly string[] DefaultPrefixes = new[] { "_" };
    private static readonly string[] DefaultSuffixes = new[] { "Dto", "Entity", "Model", "ViewModel" };

    /// <summary>
    /// Extracts global configuration from the assembly-level MapperConfigurationAttribute.
    /// </summary>
    public static MappingOptions GetGlobalConfiguration(Compilation compilation)
    {
        var globalAttr = compilation.Assembly.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "MapperConfigurationAttribute");

        if (globalAttr == null)
        {
            return GetDefaultConfiguration();
        }

        var customPrefixes = GetStringArray(globalAttr, "CustomPrefixes") ?? DefaultPrefixes;
        var customSuffixes = GetStringArray(globalAttr, "CustomSuffixes") ?? DefaultSuffixes;
        var enablePrefixSuffix = GetBool(globalAttr, "EnablePrefixSuffixNormalization") ?? true;
        var caseInsensitive = GetBool(globalAttr, "CaseInsensitiveMatching") ?? true;
        var nullableFallback = GetEnum<NullableFallbackBehavior>(globalAttr, "NullableFallbackBehavior") ?? NullableFallbackBehavior.Default;
        var allowRecursive = GetBool(globalAttr, "AllowRecursiveMapping") ?? false;
        var maxDepth = GetInt(globalAttr, "MaxRecursionDepth") ?? 5;
        var ctorStrategy = GetEnum<ConstructorSelectionStrategy>(globalAttr, "ConstructorSelectionStrategy") ?? ConstructorSelectionStrategy.Parameterless;

        return new MappingOptions(
            customPrefixes.ToImmutableArray(),
            customSuffixes.ToImmutableArray(),
            enablePrefixSuffix,
            caseInsensitive,
            nullableFallback,
            allowRecursive,
            maxDepth,
            ctorStrategy,
            MappingDirection.Both,
            false,
            ImmutableArray<ExplicitPropertyMapping>.Empty);
    }

    /// <summary>
    /// Resolves configuration for a specific mapping by merging global config with attribute properties.
    /// </summary>
    public static MappingOptions ResolveConfiguration(
        MappingOptions globalConfig,
        AttributeData mapAttribute,
        ImmutableArray<AttributeData> classAttributes,
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        // Start with global config
        var customPrefixes = globalConfig.CustomPrefixes;
        var customSuffixes = globalConfig.CustomSuffixes;
        var enablePrefixSuffix = globalConfig.EnablePrefixSuffixNormalization;
        var caseInsensitive = globalConfig.CaseInsensitiveMatching;
        var nullableFallback = globalConfig.NullableFallbackBehavior;
        var allowRecursive = globalConfig.AllowRecursiveMapping;
        var maxDepth = globalConfig.MaxRecursionDepth;
        var ctorStrategy = globalConfig.ConstructorSelectionStrategy;
        var mappingDirection = MappingDirection.Both;
        var generateHooks = false;

        // Override with MapAttribute properties
        if (GetStringArray(mapAttribute, "CustomPrefixes") is string[] attrPrefixes)
            customPrefixes = attrPrefixes.ToImmutableArray();
        if (GetStringArray(mapAttribute, "CustomSuffixes") is string[] attrSuffixes)
            customSuffixes = attrSuffixes.ToImmutableArray();
        if (GetNullableBool(mapAttribute, "EnablePrefixSuffixNormalization") is bool attrEnablePS)
            enablePrefixSuffix = attrEnablePS;
        if (GetNullableBool(mapAttribute, "CaseInsensitiveMatching") is bool attrCaseIns)
            caseInsensitive = attrCaseIns;
        if (GetNullableEnum<NullableFallbackBehavior>(mapAttribute, "NullableFallbackBehavior") is NullableFallbackBehavior attrNullable)
            nullableFallback = attrNullable;
        if (GetNullableBool(mapAttribute, "AllowRecursiveMapping") is bool attrRecursive)
            allowRecursive = attrRecursive;
        if (GetNullableInt(mapAttribute, "MaxRecursionDepth") is int attrMaxDepth)
            maxDepth = attrMaxDepth;
        if (GetNullableEnum<ConstructorSelectionStrategy>(mapAttribute, "ConstructorSelectionStrategy") is ConstructorSelectionStrategy attrCtor)
            ctorStrategy = attrCtor;

        mappingDirection = GetEnum<MappingDirection>(mapAttribute, "MappingDirection") ?? MappingDirection.Both;
        generateHooks = GetBool(mapAttribute, "GenerateMappingHooks") ?? false;

        // Check for MapOptionsAttribute on the class
        var mapOptionsAttr = classAttributes.FirstOrDefault(a => 
            a.AttributeClass?.Name == "MapOptionsAttribute" &&
            IsForThisMapping(a, sourceType, targetType));

        if (mapOptionsAttr != null)
        {
            if (GetStringArray(mapOptionsAttr, "CustomPrefixes") is string[] optPrefixes)
                customPrefixes = optPrefixes.ToImmutableArray();
            if (GetStringArray(mapOptionsAttr, "CustomSuffixes") is string[] optSuffixes)
                customSuffixes = optSuffixes.ToImmutableArray();
            if (GetNullableBool(mapOptionsAttr, "EnablePrefixSuffixNormalization") is bool optEnablePS)
                enablePrefixSuffix = optEnablePS;
            if (GetNullableBool(mapOptionsAttr, "CaseInsensitiveMatching") is bool optCaseIns)
                caseInsensitive = optCaseIns;
            if (GetNullableEnum<NullableFallbackBehavior>(mapOptionsAttr, "NullableFallbackBehavior") is NullableFallbackBehavior optNullable)
                nullableFallback = optNullable;
            if (GetNullableBool(mapOptionsAttr, "AllowRecursiveMapping") is bool optRecursive)
                allowRecursive = optRecursive;
            if (GetNullableInt(mapOptionsAttr, "MaxRecursionDepth") is int optMaxDepth)
                maxDepth = optMaxDepth;
            if (GetNullableEnum<ConstructorSelectionStrategy>(mapOptionsAttr, "ConstructorSelectionStrategy") is ConstructorSelectionStrategy optCtor)
                ctorStrategy = optCtor;

            var optDirection = GetEnum<MappingDirection>(mapOptionsAttr, "MappingDirection");
            if (optDirection.HasValue)
                mappingDirection = optDirection.Value;
            var optHooks = GetBool(mapOptionsAttr, "GenerateMappingHooks");
            if (optHooks.HasValue)
                generateHooks = optHooks.Value;
        }

        // Extract explicit property mappings
        var explicitMappings = classAttributes
            .Where(a => a.AttributeClass?.Name == "MapPropertyAttribute")
            .Where(a => IsForThisMapping(a, sourceType, targetType))
            .Select(a => 
            {
                // Try constructor arguments first (old style)
                if (a.ConstructorArguments.Length >= 2)
                {
                    var sourceProp = a.ConstructorArguments[0].Value?.ToString() ?? "";
                    var targetProp = a.ConstructorArguments[1].Value?.ToString() ?? "";
                    return new ExplicitPropertyMapping(sourceProp, targetProp);
                }
                // Try named arguments
                var srcArg = a.NamedArguments.FirstOrDefault(na => na.Key == "DomainPropName" || na.Key == "SourceProperty");
                var tgtArg = a.NamedArguments.FirstOrDefault(na => na.Key == "DtoPropName" || na.Key == "TargetProperty");
                return new ExplicitPropertyMapping(
                    srcArg.Value.Value?.ToString() ?? "",
                    tgtArg.Value.Value?.ToString() ?? "");
            })
            .Where(m => !string.IsNullOrEmpty(m.SourceProperty) && !string.IsNullOrEmpty(m.TargetProperty))
            .ToImmutableArray();

        return new MappingOptions(
            customPrefixes,
            customSuffixes,
            enablePrefixSuffix,
            caseInsensitive,
            nullableFallback,
            allowRecursive,
            maxDepth,
            ctorStrategy,
            mappingDirection,
            generateHooks,
            explicitMappings);
    }

    private static MappingOptions GetDefaultConfiguration()
    {
        return new MappingOptions(
            DefaultPrefixes.ToImmutableArray(),
            DefaultSuffixes.ToImmutableArray(),
            true,
            true,
            NullableFallbackBehavior.Default,
            false,
            5,
            ConstructorSelectionStrategy.Parameterless,
            MappingDirection.Both,
            false,
            ImmutableArray<ExplicitPropertyMapping>.Empty);
    }

    private static bool IsForThisMapping(AttributeData attribute, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        var forMapping = attribute.NamedArguments.FirstOrDefault(a => a.Key == "ForMapping").Value;
        if (forMapping.IsNull || forMapping.Values.Length != 2)
            return true; // No filter means applies to all

        var type0 = forMapping.Values[0].Value as ITypeSymbol;
        var type1 = forMapping.Values[1].Value as ITypeSymbol;

        return SymbolEqualityComparer.Default.Equals(type0, sourceType) &&
               SymbolEqualityComparer.Default.Equals(type1, targetType);
    }

    private static string[]? GetStringArray(AttributeData attribute, string propertyName)
    {
        var arg = attribute.NamedArguments.FirstOrDefault(a => a.Key == propertyName);
        if (arg.Value.IsNull || arg.Value.Kind == TypedConstantKind.Error)
            return null;

        if (arg.Value.Kind == TypedConstantKind.Array)
        {
            return arg.Value.Values.Select(v => v.Value?.ToString()).Where(s => s != null).ToArray()!;
        }

        return null;
    }

    private static string? GetString(AttributeData attribute, string propertyName)
    {
        var arg = attribute.ConstructorArguments.Concat(attribute.NamedArguments.Select(a => a.Value))
            .Skip(attribute.ConstructorArguments.Length > 0 && propertyName == "SourceProperty" ? 0 : 
                  attribute.ConstructorArguments.Length > 1 && propertyName == "TargetProperty" ? 1 : -1)
            .FirstOrDefault();

        if (arg.IsNull || arg.Kind == TypedConstantKind.Error)
        {
            arg = attribute.NamedArguments.FirstOrDefault(a => a.Key == propertyName).Value;
        }

        return arg.Value?.ToString();
    }

    private static bool? GetBool(AttributeData attribute, string propertyName)
    {
        var arg = attribute.NamedArguments.FirstOrDefault(a => a.Key == propertyName);
        if (arg.Value.IsNull || arg.Value.Kind == TypedConstantKind.Error)
            return null;

        return arg.Value.Value as bool?;
    }

    private static bool? GetNullableBool(AttributeData attribute, string propertyName)
    {
        return GetBool(attribute, propertyName);
    }

    private static int? GetInt(AttributeData attribute, string propertyName)
    {
        var arg = attribute.NamedArguments.FirstOrDefault(a => a.Key == propertyName);
        if (arg.Value.IsNull || arg.Value.Kind == TypedConstantKind.Error)
            return null;

        return arg.Value.Value as int?;
    }

    private static int? GetNullableInt(AttributeData attribute, string propertyName)
    {
        return GetInt(attribute, propertyName);
    }

    private static T? GetEnum<T>(AttributeData attribute, string propertyName) where T : struct, System.Enum
    {
        var arg = attribute.NamedArguments.FirstOrDefault(a => a.Key == propertyName);
        if (arg.Value.IsNull || arg.Value.Kind == TypedConstantKind.Error)
            return null;

        if (arg.Value.Value is int intValue)
        {
            return (T)(object)intValue;
        }

        return null;
    }

    private static T? GetNullableEnum<T>(AttributeData attribute, string propertyName) where T : struct, System.Enum
    {
        return GetEnum<T>(attribute, propertyName);
    }
}
