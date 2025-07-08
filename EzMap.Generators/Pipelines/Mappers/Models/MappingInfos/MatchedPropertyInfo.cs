using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.MappingInfos
{
    internal class MatchedPropertyInfo : PropertyInfo
    {
        public new required IPropertySymbol DomainProperty
        {
            get => base.DomainProperty!;
            init => base.DomainProperty = value;
        }

        public new required IPropertySymbol DtoProperty
        {
            get => base.DtoProperty!;
            init => base.DtoProperty = value;
        }
    }
}