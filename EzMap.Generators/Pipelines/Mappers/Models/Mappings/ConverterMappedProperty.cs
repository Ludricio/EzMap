using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.Mappings
{
    internal record ConverterMappedProperty(
        IPropertySymbol DomainSymbol,
        IPropertySymbol DtoSymbol,
        INamedTypeSymbol TypeConverterSymbol
    ) : PropertyMapping
    {
        public string ConverterInstanceName { get; } =
            $"_{char.ToLowerInvariant(TypeConverterSymbol.Name[0])}{TypeConverterSymbol.Name.Substring(1)}";
    }
}