using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.Mappings
{
    internal record UnmappedDomainProperty(
        IPropertySymbol? Symbol
    ) : UnamappedProperty(Symbol);
}