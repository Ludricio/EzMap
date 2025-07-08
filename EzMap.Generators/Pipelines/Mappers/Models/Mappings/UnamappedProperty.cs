using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.Mappings
{
    internal abstract record UnamappedProperty(
        IPropertySymbol? Symbol
    ) : PropertyMapping;
}