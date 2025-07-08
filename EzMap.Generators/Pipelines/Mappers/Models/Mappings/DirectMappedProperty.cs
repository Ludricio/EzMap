using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.Mappings
{
    internal record DirectMappedProperty(
        IPropertySymbol DomainSymbol,
        IPropertySymbol DtoSymbol
    ) : PropertyMapping;
}