using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Diagnostics;

/// <summary>
/// Contains all diagnostic descriptors for the EzMap generator.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "EzMap";

    // Errors (prevent generation)
    public static readonly DiagnosticDescriptor TargetMustBeStaticPartialClass = new(
        id: "EZMAP001",
        title: "Target must be a static partial class",
        messageFormat: "The Map attribute can only be applied to static partial classes, but '{0}' is not",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The class decorated with the Map attribute must be declared as both static and partial.");

    public static readonly DiagnosticDescriptor InvalidGenericTypeArgument = new(
        id: "EZMAP002",
        title: "Generic type arguments must be valid types",
        messageFormat: "The type argument '{0}' is not a valid type for mapping",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All generic type arguments in the Map attribute must be valid, accessible types.");

    public static readonly DiagnosticDescriptor TypesMustHaveAccessibleConstructors = new(
        id: "EZMAP003",
        title: "Types must have accessible constructors",
        messageFormat: "The type '{0}' does not have an accessible parameterless constructor",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Both types in the mapping must have accessible parameterless constructors.");

    public static readonly DiagnosticDescriptor CircularMappingDependency = new(
        id: "EZMAP004",
        title: "Circular mapping dependency detected",
        messageFormat: "Circular mapping dependency detected: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The mapping configuration contains circular dependencies that cannot be resolved.");

    // Warnings
    public static readonly DiagnosticDescriptor UnmatchedSourceProperty = new(
        id: "EZMAP101",
        title: "Source property has no matching target property",
        messageFormat: "Property '{0}' in source type has no matching property in target type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A property in the source type could not be mapped to any property in the target type.");

    public static readonly DiagnosticDescriptor PropertyTypeMismatch = new(
        id: "EZMAP102",
        title: "Property type mismatch requires explicit configuration",
        messageFormat: "Property '{0}' has incompatible types that cannot be automatically mapped: {1} -> {2}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The property types are incompatible and require explicit mapping configuration.");

    public static readonly DiagnosticDescriptor NullableToNonNullableMapping = new(
        id: "EZMAP103",
        title: "Nullable to non-nullable mapping may produce unexpected defaults",
        messageFormat: "Mapping nullable property '{0}' to non-nullable target may use default value",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Mapping from a nullable property to a non-nullable property will use the default value when the source is null.");

    public static readonly DiagnosticDescriptor RecursiveMappingDetected = new(
        id: "EZMAP104",
        title: "Recursive mapping detected",
        messageFormat: "Recursive mapping detected for property '{0}' - ensure a mapping exists for types {1} -> {2}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A property requires recursive mapping. Ensure that a mapping is configured for the property types.");

    // Info
    public static readonly DiagnosticDescriptor UsingNormalizedNameMatch = new(
        id: "EZMAP201",
        title: "Using normalized name match",
        messageFormat: "Using normalized name match for property '{0}' -> '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Properties are being matched using name normalization (removing prefixes/suffixes).");

    public static readonly DiagnosticDescriptor UsingExplicitCast = new(
        id: "EZMAP202",
        title: "Using explicit cast",
        messageFormat: "Using explicit cast for property '{0}': {1} -> {2}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "An explicit cast will be used to convert the property value.");
}
