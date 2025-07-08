using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.Mappings
{
    internal record DefaultDtoMappedProperty(
        IPropertySymbol Symbol,
        string DefaultValue
    ) : DefaultMappedProperty(Symbol, DefaultValue);
}