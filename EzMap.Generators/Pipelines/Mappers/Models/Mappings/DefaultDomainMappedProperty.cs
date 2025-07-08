using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.Mappings
{
    internal record DefaultDomainMappedProperty(
        IPropertySymbol Symbol,
        string DefaultValue
    ) : DefaultMappedProperty(Symbol, DefaultValue);
}