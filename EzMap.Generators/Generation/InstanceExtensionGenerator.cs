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
        // Generate Source -> Target mapping method (if mappings exist)
        if (!sourceToTargetMappings.IsDefaultOrEmpty)
        {
            GenerateMappingMethod(
                builder,
                config.SourceType,
                config.TargetType,
                sourceToTargetMappings,
                GetMethodName(config.SourceType, config.TargetType),
                config.Options);

            if (!targetToSourceMappings.IsDefaultOrEmpty)
                builder.AppendLine();
        }

        // Generate Target -> Source mapping method (if mappings exist)
        if (!targetToSourceMappings.IsDefaultOrEmpty)
        {
            GenerateMappingMethod(
                builder,
                config.TargetType,
                config.SourceType,
                targetToSourceMappings,
                GetMethodName(config.TargetType, config.SourceType),
                config.Options);
        }
    }

    private static void GenerateMappingMethod(
        CodeBuilder builder,
        TypeToMap sourceType,
        TypeToMap targetType,
        ImmutableArray<PropertyMapping> mappings,
        string methodName,
        MappingOptions options)
    {
        builder.AppendLine("/// <summary>");
        builder.AppendLine($"/// Maps from {sourceType.SimpleName} to {targetType.SimpleName}.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine($"public static {targetType.FullyQualifiedName} {methodName}(this {sourceType.FullyQualifiedName} source)");
        builder.AppendOpenBrace();

        // Generate BeforeMap hook if enabled
        if (options.GenerateMappingHooks)
        {
            builder.AppendLine($"source = BeforeMap(source);");
            builder.AppendLine();
        }

        // Generate the mapping logic
        builder.AppendLine($"var target = new {targetType.FullyQualifiedName}");
        builder.AppendOpenBrace();

        for (int i = 0; i < mappings.Length; i++)
        {
            var mapping = mappings[i];
            var isLast = i == mappings.Length - 1;
            
            GeneratePropertyMapping(builder, mapping, isLast, options);
        }

        builder.AppendCloseBrace();
        builder.AppendLine(";");
        builder.AppendLine();

        // Generate AfterMap hook if enabled
        if (options.GenerateMappingHooks)
        {
            builder.AppendLine($"target = AfterMap(source, target);");
            builder.AppendLine();
        }

        builder.AppendLine("return target;");
        builder.AppendCloseBrace();

        // Generate hook method stubs if enabled
        if (options.GenerateMappingHooks)
        {
            builder.AppendLine();
            builder.AppendLine($"static partial {sourceType.FullyQualifiedName} BeforeMap({sourceType.FullyQualifiedName} source);");
            builder.AppendLine();
            builder.AppendLine($"static partial {targetType.FullyQualifiedName} AfterMap({sourceType.FullyQualifiedName} source, {targetType.FullyQualifiedName} target);");
        }
    }

    private static void GeneratePropertyMapping(
        CodeBuilder builder, 
        PropertyMapping mapping, 
        bool isLast, 
        MappingOptions options)
    {
        var sourceAccess = $"source.{mapping.SourcePropertyName}";
        string valueExpression;

        switch (mapping.Strategy)
        {
            case PropertyMappingStrategy.DirectAssignment:
                if (mapping.RequiresNullHandling)
                {
                    valueExpression = GenerateNullHandling(sourceAccess, mapping.TargetPropertyType, options);
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
                    var nullHandled = GenerateNullHandling(sourceAccess, mapping.SourcePropertyType, options);
                    valueExpression = $"({mapping.TargetPropertyType})({nullHandled})";
                }
                else
                {
                    valueExpression = $"({mapping.TargetPropertyType}){sourceAccess}";
                }
                break;

            case PropertyMappingStrategy.RecursiveMapping:
                // Call the mapper method
                if (!string.IsNullOrEmpty(mapping.MapperMethodName))
                {
                    if (mapping.RequiresNullHandling)
                    {
                        valueExpression = $"{sourceAccess}?.{mapping.MapperMethodName}() ?? default({mapping.TargetPropertyType})";
                    }
                    else
                    {
                        valueExpression = $"{sourceAccess}.{mapping.MapperMethodName}()";
                    }
                }
                else
                {
                    // Fallback if method name not provided
                    valueExpression = $"default({mapping.TargetPropertyType})";
                }
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

    private static string GenerateNullHandling(string sourceAccess, string targetType, MappingOptions options)
    {
        return options.NullableFallbackBehavior switch
        {
            NullableFallbackBehavior.Default => $"{sourceAccess} ?? default({targetType})",
            NullableFallbackBehavior.Throw => $"{sourceAccess} ?? throw new global::System.ArgumentNullException(nameof({sourceAccess}))",
            NullableFallbackBehavior.Diagnostic => $"{sourceAccess} ?? default({targetType})", // Diagnostic already emitted
            _ => $"{sourceAccess} ?? default({targetType})"
        };
    }

    private static string GetMethodName(TypeToMap sourceType, TypeToMap targetType)
    {
        return $"MapTo{targetType.SimpleName}";
    }
}
