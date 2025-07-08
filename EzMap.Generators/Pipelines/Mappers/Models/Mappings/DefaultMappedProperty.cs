using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.Mappings
{
    internal abstract record DefaultMappedProperty(
        IPropertySymbol Symbol,
        string DefaultValue
    ) : PropertyMapping;
}