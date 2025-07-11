using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.Mappings
{
    internal abstract record UnmappedProperty(
        IPropertySymbol Symbol
    ) : PropertyMapping;
}