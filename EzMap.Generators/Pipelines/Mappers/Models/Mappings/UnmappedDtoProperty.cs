using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.Mappings
{
    internal record UnmappedDtoProperty(
        IPropertySymbol Symbol
    ) : UnmappedProperty(Symbol);
}