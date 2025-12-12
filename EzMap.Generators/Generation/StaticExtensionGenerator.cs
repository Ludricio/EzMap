using EzMap.Generators.Models;
using System.Collections.Immutable;

namespace EzMap.Generators.Generation;

/// <summary>
/// Generates static extension members using new C# extension syntax.
/// NOTE: This feature is not yet available in released C# versions.
/// </summary>
internal static class StaticExtensionGenerator
{
    /// <summary>
    /// Generates static extension members for bidirectional mapping.
    /// </summary>
    public static void Generate(
        CodeBuilder builder,
        MapperConfiguration config,
        ImmutableArray<PropertyMapping> sourceToTargetMappings,
        ImmutableArray<PropertyMapping> targetToSourceMappings)
    {
        // Generate extension for Source type
        GenerateExtension(
            builder,
            config.SourceType,
            config.TargetType,
            targetToSourceMappings); // Note: reversed because we're creating Source from Target

        builder.AppendLine();

        // Generate extension for Target type
        GenerateExtension(
            builder,
            config.TargetType,
            config.SourceType,
            sourceToTargetMappings); // Note: reversed because we're creating Target from Source
    }

    private static void GenerateExtension(
        CodeBuilder builder,
        TypeToMap extendedType,
        TypeToMap sourceType,
        ImmutableArray<PropertyMapping> mappings)
    {
        // NOTE: This syntax is for future C# versions
        // For now, we'll comment it out and generate a placeholder
        
        builder.AppendLine($"// Static extension for {extendedType.SimpleName} (requires C# extension member syntax support)");
        builder.AppendLine($"// extension({extendedType.FullyQualifiedName})");
        builder.AppendLine("// {");
        builder.AppendLine($"//     public static {extendedType.FullyQualifiedName} From({sourceType.FullyQualifiedName} source)");
        builder.AppendLine("//     {");
        builder.AppendLine($"//         return new {extendedType.FullyQualifiedName}");
        builder.AppendLine("//         {");
        
        foreach (var mapping in mappings)
        {
            builder.AppendLine($"//             {mapping.TargetPropertyName} = source.{mapping.SourcePropertyName},");
        }
        
        builder.AppendLine("//         };");
        builder.AppendLine("//     }");
        builder.AppendLine("// }");
    }

    /// <summary>
    /// Checks if the compilation supports extension member syntax.
    /// </summary>
    public static bool SupportsExtensionMembers(Microsoft.CodeAnalysis.Compilation compilation)
    {
        // Feature detection: check language version
        // For now, this always returns false as the feature is not yet available
        // In the future, check for C# 13 or later
        return false;
    }
}
