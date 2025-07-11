using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Diagnostics
{
    internal static class EzMapDiagnostic
    {
        private const string Category = "EzMap";

        public static DiagnosticDescriptor MapClassAttributeTargetNotClassOrStruct { get; } = Create(
            "EMG001",
            "Attribute target is not a class or struct",
            "The target of the MapClassAttribute must be a class or struct."
        );

        public static DiagnosticDescriptor MapClassAttributeNotFound { get; } = Create(
            "EMG002",
            "MapClassAttribute not found",
            "The MapClassAttribute is not found on the target class {0}, make sure you have the EzMap package installed."
        );

        public static DiagnosticDescriptor MissingPropertyOnMappedClass { get; } = Create(
            "EMG003",
            "Mapped property not found on class",
            "The mapped property {0} is not found on the class {1}. Make sure the property exists and is accessible."
        );

        public static DiagnosticDescriptor MappedPropertyIsStatic { get; } = Create(
            "EMG004",
            "Mapped property is static",
            "The mapped property {0} on class {1} cannot be static. Static properties are not supported."
        );

        public static DiagnosticDescriptor MappedPropertyIsReadOnly { get; } = Create(
            "EMG005",
            "Mapped property is read-only",
            "The mapped property {0} on class {1} cannot be read-only. Read-only properties are not supported."
        );

        public static DiagnosticDescriptor MappedPropertyIsUnaccessible { get; } = Create(
            "EMG006",
            "Mapped property is inaccessible",
            "The mapped property {0} on class {1} is not accessible. Make sure the property is public or internal."
        );

        public static DiagnosticDescriptor MapPropertyAttributeInvalidArguments { get; } = Create(
            "EMG007",
            "Invalid arguments in MapClassAttribute",
            "The MapClassAttribute on class {0} has invalid arguments. Ensure the arguments are correct and match the expected types."
        );

        public static DiagnosticDescriptor MapPropertyAttributeArgumentIsNotClassOrStruct { get; } = Create(
            "EMG008",
            "MapClassAttribute argument is not a class or struct",
            "The arguments for MapClassAttribute must be classes or structs. Ensure the argument types are correct."
        );

        public static DiagnosticDescriptor DefaultConverterSettingsTargetNotClass { get; } = Create(
            "EMG101",
            "DefaultConverterSettingsAttribute target is not a class",
            "The target of the DefaultConverterSettingsAttribute pipeline must be a class."
        );

        public static DiagnosticDescriptor DefaultConverterSettingsGenericTypeNotSupported { get; } = Create(
            "EMG102",
            "Generic types are not supported",
            "The class {0} is a generic type, which is not supported for DefaultConverterSettingsAttribute."
        );

        public static DiagnosticDescriptor DefaultConverterSettingsAbstractClassNotSupported { get; } = Create(
            "EMG103",
            "Abstract classes are not supported",
            "The class {0} is abstract, which is not supported for DefaultConverterSettingsAttribute."
        );

        public static DiagnosticDescriptor DefaultConverterSettingsStaticClassNotSupported { get; } = Create(
            "EMG104",
            "Static classes are not supported",
            "The class {0} is static, which is not supported for DefaultConverterSettings."
        );

        public static DiagnosticDescriptor DefaultConverterSettingsInvalidAccessibility { get; } = Create(
            "EMG105",
            "Invalid accessibility",
            "The class {0} must be public or internal for DefaultConverterSettings."
        );

        public static DiagnosticDescriptor DefaultConverterSettingsMissingDefaultConstructor { get; } = Create(
            "EMG106",
            "Missing default constructor",
            "The class {0} must have a public or internal parameterless constructor for DefaultConverterSettingsAttribute."
        );

        public static DiagnosticDescriptor MultipleDefaultConverterSettingsFound { get; } = Create(
            "EMG107",
            "Multiple DefaultConverterSettings found",
            "Multiple classes with DefaultConverterSettingsAttribute instances found in assembly. Only one is allowed per assembly."
        );
        
        public static DiagnosticDescriptor RequiredPropertyNotMapped { get; } = Create(
            "EMG108",
            "Required property not mapped",
            "The required property {0} on class {1} is not mapped. Ensure all required properties are mapped."
        );

        private static DiagnosticDescriptor Create(
            string id,
            string title,
            string messageFormat,
            DiagnosticSeverity severity = DiagnosticSeverity.Error
        )
        {
            return new DiagnosticDescriptor(
                id,
                title,
                messageFormat,
                Category,
                severity,
                isEnabledByDefault: true
            );
        }
    }
}