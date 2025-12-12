using EzMap.Generators.Diagnostics;
using EzMap.Generators.Generation;
using EzMap.Generators.Models;
using EzMap.Generators.Pipeline;
using EzMap.Generators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace EzMap.Generators;

/// <summary>
/// Source generator for EzMap mapping methods.
/// </summary>
[Generator]
public class MapGenerator : IIncrementalGenerator
{
    private const string MapAttributeName = "EzMap.MapAttribute`2";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Stage 1: Syntax filter - find classes with Map attribute
        var mapAttributeProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MapAttributeName,
                predicate: SyntaxFilter,
                transform: SemanticTransform)
            .WithTrackingName("MapAttribute_Extraction")
            .Where(static context => context is not null)
            .Collect()
            .WithTrackingName("MapAttribute_Collection");

        // Stage 2: Combine with compilation for type checking
        var compilationAndMappings = context.CompilationProvider
            .Combine(mapAttributeProvider)
            .WithTrackingName("Compilation_Combine");

        // Stage 3: Generate source
        context.RegisterSourceOutput(compilationAndMappings, GenerateSource);
    }

    /// <summary>
    /// Quick syntax filter to identify potential Map attribute targets.
    /// </summary>
    private static bool SyntaxFilter(SyntaxNode node, CancellationToken cancellationToken)
    {
        // Only process class declarations
        if (node is not ClassDeclarationSyntax classDecl)
            return false;

        // Must be static and partial
        var modifiers = classDecl.Modifiers;
        return modifiers.Any(m => m.ValueText == "static") &&
               modifiers.Any(m => m.ValueText == "partial");
    }

    /// <summary>
    /// Semantic transform to extract configuration from the attribute.
    /// </summary>
    private static MapperGenerationModel? SemanticTransform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        if (context.TargetNode is not ClassDeclarationSyntax classDecl)
            return null;

        var classSymbol = context.TargetSymbol as INamedTypeSymbol;
        if (classSymbol == null)
            return null;

        // Validate: must be static partial class
        if (!classSymbol.IsStatic || !classDecl.Modifiers.Any(m => m.ValueText == "partial"))
        {
            return null;
        }

        var attribute = context.Attributes.FirstOrDefault();
        if (attribute == null || attribute.AttributeClass == null)
            return null;

        // Extract generic type arguments
        var attributeClass = attribute.AttributeClass;
        if (attributeClass.TypeArguments.Length != 2)
            return null;

        var sourceType = attributeClass.TypeArguments[0];
        var targetType = attributeClass.TypeArguments[1];

        // Check for errors in type arguments (like unresolved types)
        if (sourceType.TypeKind == TypeKind.Error || targetType.TypeKind == TypeKind.Error)
            return null;

        // Extract configuration properties
        bool generateInstanceExtensions = true;
        bool generateStaticExtensions = false;

        foreach (var namedArg in attribute.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "GenerateInstanceExtensions":
                    if (namedArg.Value.Value is bool instValue)
                        generateInstanceExtensions = instValue;
                    break;
                case "GenerateStaticExtensions":
                    if (namedArg.Value.Value is bool staticValue)
                        generateStaticExtensions = staticValue;
                    break;
            }
        }

        // Note: Full configuration resolution happens later in GenerateSource
        // when we have access to the Compilation for global config

        // Create type models
        var sourceTypeModel = new TypeToMap(
            SymbolHelpers.GetFullyQualifiedName(sourceType),
            sourceType.Name,
            NullabilityHelper.IsNullable(sourceType));

        var targetTypeModel = new TypeToMap(
            SymbolHelpers.GetFullyQualifiedName(targetType),
            targetType.Name,
            NullabilityHelper.IsNullable(targetType));

        var className = classSymbol.Name;
        var classNamespace = classSymbol.ContainingNamespace?.IsGlobalNamespace == true 
            ? "" 
            : (classSymbol.ContainingNamespace?.ToDisplayString() ?? "");

        // Create a placeholder configuration without options for now
        // Options will be resolved in GenerateSource
        var placeholderOptions = new MappingOptions(
            ImmutableArray<string>.Empty,
            ImmutableArray<string>.Empty,
            true,
            true,
            NullableFallbackBehavior.Default,
            false,
            5,
            ConstructorSelectionStrategy.Parameterless,
            MappingDirection.Both,
            false,
            ImmutableArray<ExplicitPropertyMapping>.Empty);

        var configuration = new MapperConfiguration(
            className,
            classNamespace,
            sourceTypeModel,
            targetTypeModel,
            generateInstanceExtensions,
            generateStaticExtensions,
            placeholderOptions,
            attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None);

        return new MapperGenerationModel(configuration, sourceType, targetType, classSymbol);
    }

    /// <summary>
    /// Generates the source code for all mappings.
    /// </summary>
    private static void GenerateSource(
        SourceProductionContext context,
        (Compilation compilation, ImmutableArray<MapperGenerationModel?> models) input)
    {
        var (compilation, models) = input;

        // Get global configuration once
        var globalConfig = ConfigurationResolver.GetGlobalConfiguration(compilation);

        foreach (var model in models)
        {
            if (model == null)
                continue;

            GenerateMapperClass(context, compilation, model, globalConfig);
        }
    }

    private static void GenerateMapperClass(
        SourceProductionContext context,
        Compilation compilation,
        MapperGenerationModel model,
        MappingOptions globalConfig)
    {
        var diagnostics = new List<Diagnostic>();
        
        // Get all attributes on the mapper class for resolving options
        var classAttributes = model.ClassSymbol.GetAttributes();
        
        // Find the specific Map attribute for this mapping
        var mapAttribute = classAttributes.FirstOrDefault(a => 
            a.AttributeClass?.Name == "MapAttribute" &&
            a.AttributeClass.TypeArguments.Length == 2 &&
            SymbolEqualityComparer.Default.Equals(a.AttributeClass.TypeArguments[0], model.SourceTypeSymbol) &&
            SymbolEqualityComparer.Default.Equals(a.AttributeClass.TypeArguments[1], model.TargetTypeSymbol));

        if (mapAttribute == null)
            return;

        // Resolve full configuration for this mapping
        var resolvedOptions = ConfigurationResolver.ResolveConfiguration(
            globalConfig,
            mapAttribute,
            classAttributes,
            model.SourceTypeSymbol,
            model.TargetTypeSymbol);

        // Update configuration with resolved options
        var config = new MapperConfiguration(
            model.Configuration.ClassName,
            model.Configuration.ClassNamespace,
            model.Configuration.SourceType,
            model.Configuration.TargetType,
            model.Configuration.GenerateInstanceExtensions,
            model.Configuration.GenerateStaticExtensions,
            resolvedOptions,
            model.Configuration.AttributeLocation);

        // Validate constructors (consider constructor selection strategy)
        if (resolvedOptions.ConstructorSelectionStrategy == ConstructorSelectionStrategy.Parameterless)
        {
            if (!SymbolHelpers.HasAccessibleParameterlessConstructor(model.SourceTypeSymbol, model.ClassSymbol))
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.TypesMustHaveAccessibleConstructors,
                    config.AttributeLocation,
                    config.SourceType.SimpleName));
            }

            if (!SymbolHelpers.HasAccessibleParameterlessConstructor(model.TargetTypeSymbol, model.ClassSymbol))
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.TypesMustHaveAccessibleConstructors,
                    config.AttributeLocation,
                    config.TargetType.SimpleName));
            }
        }

        // Match properties with configuration
        ImmutableArray<PropertyMapping> sourceToTargetMappings = ImmutableArray<PropertyMapping>.Empty;
        ImmutableArray<PropertyMapping> targetToSourceMappings = ImmutableArray<PropertyMapping>.Empty;

        // Generate based on mapping direction
        if (resolvedOptions.MappingDirection == MappingDirection.Both || 
            resolvedOptions.MappingDirection == MappingDirection.SourceToTarget)
        {
            var (mappings, diags) = PropertyMatcher.MatchProperties(
                model.SourceTypeSymbol, 
                model.TargetTypeSymbol, 
                compilation, 
                config.AttributeLocation,
                resolvedOptions);
            sourceToTargetMappings = mappings;
            diagnostics.AddRange(diags);
        }

        if (resolvedOptions.MappingDirection == MappingDirection.Both || 
            resolvedOptions.MappingDirection == MappingDirection.TargetToSource)
        {
            var (mappings, diags) = PropertyMatcher.MatchProperties(
                model.TargetTypeSymbol, 
                model.SourceTypeSymbol, 
                compilation, 
                config.AttributeLocation,
                resolvedOptions);
            targetToSourceMappings = mappings;
            diagnostics.AddRange(diags);
        }

        // Report diagnostics
        foreach (var diagnostic in diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }

        // Generate code
        var generationContext = new GenerationContext(
            config,
            sourceToTargetMappings,
            targetToSourceMappings,
            diagnostics.ToImmutableArray());

        var sourceCode = GenerateSourceCode(generationContext, compilation);
        
        // Add source to compilation
        var fileName = $"{config.ClassName}_{config.SourceType.SimpleName}_{config.TargetType.SimpleName}.g.cs";
        context.AddSource(fileName, sourceCode);
    }

    private static string GenerateSourceCode(GenerationContext context, Compilation compilation)
    {
        var builder = new CodeBuilder();

        // File header
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();

        // Namespace (only if not empty or global namespace)
        var hasNamespace = !string.IsNullOrEmpty(context.Configuration.ClassNamespace);
        if (hasNamespace)
        {
            builder.AppendLine($"namespace {context.Configuration.ClassNamespace}");
            builder.AppendOpenBrace();
        }

        // Class declaration
        builder.AppendLine($"public static partial class {context.Configuration.ClassName}");
        builder.AppendOpenBrace();

        // Generate instance extensions if configured
        if (context.Configuration.GenerateInstanceExtensions)
        {
            InstanceExtensionGenerator.Generate(
                builder,
                context.Configuration,
                context.SourceToTargetMappings,
                context.TargetToSourceMappings);
        }

        // Generate static extensions if configured and supported
        if (context.Configuration.GenerateStaticExtensions)
        {
            if (context.Configuration.GenerateInstanceExtensions)
                builder.AppendLine();

            StaticExtensionGenerator.Generate(
                builder,
                context.Configuration,
                context.SourceToTargetMappings,
                context.TargetToSourceMappings);
        }

        // Close class
        builder.AppendCloseBrace();

        // Close namespace (only if we opened one)
        if (hasNamespace)
        {
            builder.AppendCloseBrace();
        }

        return builder.ToString();
    }
}
