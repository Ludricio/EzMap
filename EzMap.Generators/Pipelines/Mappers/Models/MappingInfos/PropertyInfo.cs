using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.MappingInfos
{
    internal abstract class PropertyInfo
    {
        protected IPropertySymbol? DomainProperty { get; init; }
        protected IPropertySymbol? DtoProperty { get; init; }
        public string? TypeConverterName { get; set; }
    }
}