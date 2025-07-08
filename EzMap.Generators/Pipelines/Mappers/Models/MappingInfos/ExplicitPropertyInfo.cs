using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.MappingInfos
{
    internal class ExplicitPropertyInfo : MatchedPropertyInfo
    {
        public required INamedTypeSymbol? TypeConverter { get; init; }
    }
}