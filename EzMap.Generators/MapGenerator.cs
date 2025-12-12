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
    private static MapperConfiguration? SemanticTransform(
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

        return new MapperConfiguration(
            className,
            classNamespace,
            sourceTypeModel,
            targetTypeModel,
            generateInstanceExtensions,
            generateStaticExtensions,
            attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None);
    }

    /// <summary>
    /// Generates the source code for all mappings.
    /// </summary>
    private static void GenerateSource(
        SourceProductionContext context,
        (Compilation compilation, ImmutableArray<MapperConfiguration?> configurations) input)
    {
        var (compilation, configurations) = input;

        foreach (var config in configurations)
        {
            if (config == null)
                continue;

            GenerateMapperClass(context, compilation, config.Value);
        }
    }

    private static void GenerateMapperClass(
        SourceProductionContext context,
        Compilation compilation,
        MapperConfiguration config)
    {
        var diagnostics = new List<Diagnostic>();

        // Get type symbols for validation and property matching
        // We need to convert the fully qualified name to metadata name format
        var sourceTypeName = config.SourceType.FullyQualifiedName
            .Replace("global::", "")
            .Replace("<", "`1[")
            .Replace(">", "]")
            .Replace(", ", ",");
        var targetTypeName = config.TargetType.FullyQualifiedName
            .Replace("global::", "")
            .Replace("<", "`1[")
            .Replace(">", "]")
            .Replace(", ", ",");

        // Try to find the types in the compilation
        var sourceType = FindTypeSymbol(compilation, config.SourceType.SimpleName);
        var targetType = FindTypeSymbol(compilation, config.TargetType.SimpleName);

        if (sourceType == null || targetType == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.InvalidGenericTypeArgument,
                config.AttributeLocation,
                sourceType == null ? config.SourceType.SimpleName : config.TargetType.SimpleName));
            return;
        }

        // Validate constructors
        var classSymbol = FindTypeSymbol(compilation, config.ClassName, config.ClassNamespace);
        if (classSymbol == null)
            return;

        if (!SymbolHelpers.HasAccessibleParameterlessConstructor(sourceType, classSymbol))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.TypesMustHaveAccessibleConstructors,
                config.AttributeLocation,
                config.SourceType.SimpleName));
        }

        if (!SymbolHelpers.HasAccessibleParameterlessConstructor(targetType, classSymbol))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.TypesMustHaveAccessibleConstructors,
                config.AttributeLocation,
                config.TargetType.SimpleName));
        }

        // Match properties
        var (sourceToTargetMappings, sourceToTargetDiagnostics) = 
            PropertyMatcher.MatchProperties(sourceType, targetType, compilation, config.AttributeLocation);
        diagnostics.AddRange(sourceToTargetDiagnostics);

        var (targetToSourceMappings, targetToSourceDiagnostics) = 
            PropertyMatcher.MatchProperties(targetType, sourceType, compilation, config.AttributeLocation);
        diagnostics.AddRange(targetToSourceDiagnostics);

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

    /// <summary>
    /// Helper to find a type symbol by name.
    /// </summary>
    private static INamedTypeSymbol? FindTypeSymbol(Compilation compilation, string typeName, string? namespaceName = null)
    {
        // Search through all types in the compilation
        var visitor = new TypeSymbolVisitor(typeName, namespaceName);
        visitor.Visit(compilation.Assembly.GlobalNamespace);
        return visitor.FoundType;
    }

    private class TypeSymbolVisitor : SymbolVisitor
    {
        private readonly string _typeName;
        private readonly string? _namespaceName;
        public INamedTypeSymbol? FoundType { get; private set; }

        public TypeSymbolVisitor(string typeName, string? namespaceName = null)
        {
            _typeName = typeName;
            _namespaceName = namespaceName;
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            if (FoundType != null)
                return;

            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
                if (FoundType != null)
                    return;
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (FoundType != null)
                return;

            if (symbol.Name == _typeName)
            {
                if (_namespaceName == null || symbol.ContainingNamespace?.ToDisplayString() == _namespaceName)
                {
                    FoundType = symbol;
                    return;
                }
            }

            // Check nested types
            foreach (var member in symbol.GetTypeMembers())
            {
                member.Accept(this);
                if (FoundType != null)
                    return;
            }
        }
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
