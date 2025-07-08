using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.MappingInfos
{
    internal class UnmatchedPropertyInfo : PropertyInfo
    {
        public new IPropertySymbol? DomainProperty
        {
            get => base.DomainProperty;
            init => base.DomainProperty = value;
        }

        public new required IPropertySymbol? DtoProperty
        {
            get => base.DtoProperty;
            init => base.DtoProperty = value;
        }

        public required string? DefaultValue { get; init; }
    }
}