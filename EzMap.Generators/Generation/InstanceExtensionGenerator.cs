using EzMap.Generators.Models;
using EzMap.Generators.Utils;
using System.Collections.Immutable;

namespace EzMap.Generators.Generation;

/// <summary>
/// Generates instance extension methods for mapping.
/// </summary>
internal static class InstanceExtensionGenerator
{
    /// <summary>
    /// Generates instance extension methods for bidirectional mapping.
    /// </summary>
    public static void Generate(
        CodeBuilder builder,
        MapperConfiguration config,
        ImmutableArray<PropertyMapping> sourceToTargetMappings,
        ImmutableArray<PropertyMapping> targetToSourceMappings)
    {
        // Generate Source -> Target mapping method
        GenerateMappingMethod(
            builder,
            config.SourceType,
            config.TargetType,
            sourceToTargetMappings,
            GetMethodName(config.SourceType, config.TargetType));

        builder.AppendLine();

        // Generate Target -> Source mapping method
        GenerateMappingMethod(
            builder,
            config.TargetType,
            config.SourceType,
            targetToSourceMappings,
            GetMethodName(config.TargetType, config.SourceType));
    }

    private static void GenerateMappingMethod(
        CodeBuilder builder,
        TypeToMap sourceType,
        TypeToMap targetType,
        ImmutableArray<PropertyMapping> mappings,
        string methodName)
    {
        builder.AppendLine("/// <summary>");
        builder.AppendLine($"/// Maps from {sourceType.SimpleName} to {targetType.SimpleName}.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine($"public static {targetType.FullyQualifiedName} {methodName}(this {sourceType.FullyQualifiedName} source)");
        builder.AppendOpenBrace();

        // Generate the mapping logic
        builder.AppendLine($"return new {targetType.FullyQualifiedName}");
        builder.AppendOpenBrace();

        for (int i = 0; i < mappings.Length; i++)
        {
            var mapping = mappings[i];
            var isLast = i == mappings.Length - 1;
            
            GeneratePropertyMapping(builder, mapping, isLast);
        }

        builder.AppendCloseBrace();
        builder.AppendLine(";");
        builder.AppendCloseBrace();
    }

    private static void GeneratePropertyMapping(CodeBuilder builder, PropertyMapping mapping, bool isLast)
    {
        var sourceAccess = $"source.{mapping.SourcePropertyName}";
        string valueExpression;

        switch (mapping.Strategy)
        {
            case PropertyMappingStrategy.DirectAssignment:
                if (mapping.RequiresNullHandling)
                {
                    // Handle nullable to non-nullable - use default for the type
                    valueExpression = $"{sourceAccess} ?? default({mapping.TargetPropertyType})";
                }
                else
                {
                    valueExpression = sourceAccess;
                }
                break;

            case PropertyMappingStrategy.ImplicitCast:
                valueExpression = sourceAccess;
                break;

            case PropertyMappingStrategy.ExplicitCast:
                if (mapping.RequiresNullHandling)
                {
                    valueExpression = $"({mapping.TargetPropertyType})({sourceAccess} ?? default({mapping.SourcePropertyType}))";
                }
                else
                {
                    valueExpression = $"({mapping.TargetPropertyType}){sourceAccess}";
                }
                break;

            case PropertyMappingStrategy.RecursiveMapping:
                // For now, just use default - would need more context
                valueExpression = $"default({mapping.TargetPropertyType})";
                break;

            case PropertyMappingStrategy.UseDefault:
                valueExpression = $"default({mapping.TargetPropertyType})";
                break;

            default:
                valueExpression = sourceAccess;
                break;
        }

        var comma = isLast ? "" : ",";
        builder.AppendLine($"{mapping.TargetPropertyName} = {valueExpression}{comma}");
    }

    private static string GetMethodName(TypeToMap sourceType, TypeToMap targetType)
    {
        return $"MapTo{targetType.SimpleName}";
    }
}
