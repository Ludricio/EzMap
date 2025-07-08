using EzMap.Generators.Pipelines.Mappers.Converters;
using EzMap.Generators.Pipelines.Mappers.Models.MappingInfos;
using EzMap.Generators.Pipelines.Mappers.Models.Mappings;
using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.PropertyMappers
{
    internal class PropertyMappingFinalizer(
        Compilation compilation,
        AutoMapConvention autoMapConvention
    )
    {
        private readonly bool _forceRefNullability = autoMapConvention.HasFlag(AutoMapConvention.ForceReferenceNullability);

        public List<PropertyMapping> MapPropertiesFromPropertyInfos(
            List<PropertyInfo> propertyMappingInfos
        )
        {
            List<PropertyMapping> finalMappings = [];
            foreach (var propInfo in propertyMappingInfos)
            {
                switch (propInfo)
                {
                    case ExplicitPropertyInfo exp:
                        HandleExplicitPropertyMappingInfo(exp, finalMappings);
                        break;
                    case MatchedPropertyInfo matched:
                        HandleMatchingPropertyMappingInfo(matched, finalMappings);
                        break;
                    case UnmatchedPropertyInfo unmatched:
                        if (unmatched.DomainProperty is not null)
                        {
                            HandleUnmatchedPropertySymbol(unmatched.DomainProperty, true, finalMappings);
                        }
                        else
                        {
                            HandleUnmatchedPropertySymbol(unmatched.DtoProperty, false, finalMappings);
                        }

                        break;
                }
            }

            return finalMappings;
        }

        private void HandleExplicitPropertyMappingInfo(
            ExplicitPropertyInfo prop,
            in List<PropertyMapping> results
        )
        {
            PropertyMapping? mapping = null;
            if (prop.TypeConverter is not null)
            {
                mapping = new ConverterMappedProperty(
                    prop.DomainProperty,
                    prop.DtoProperty,
                    prop.TypeConverter
                );
            }

            mapping ??= MapWithDirect(prop);
            mapping ??= MapWithConverter(prop);
            if (mapping is not null)
            {
                results.Add(mapping);
                return;
            }
            //TODO diagnostic, we're trying to map properties of different non-compatible types without a converter
        }

        private void HandleMatchingPropertyMappingInfo(
            MatchedPropertyInfo prop,
            in List<PropertyMapping> results
        )
        {
            PropertyMapping? mapping = MapWithDirect(prop);
            mapping ??= MapWithConverter(prop);
            if (mapping is not null)
            {
                results.Add(mapping);
                return;
            }

            HandleUnmatchedPropertySymbol(prop.DomainProperty, true, results);
            HandleUnmatchedPropertySymbol(prop.DtoProperty, false, results);
        }

        private void HandleUnmatchedPropertySymbol(
            IPropertySymbol symbol,
            bool isDomain,
            in List<PropertyMapping> results
        )
        {
            PropertyMapping? mapping = MapWithDefault(symbol, isDomain, null);
            if (mapping is not null)
            {
                results.Add(mapping);
            }
            else
            {
                results.Add(isDomain
                    ? new UnmappedDomainProperty(symbol)
                    : new UnmappedDtoProperty(symbol)
                );
            }
        }

        private DirectMappedProperty? MapWithDirect(
            MatchedPropertyInfo prop
        )
        {
            var domainProp = prop.DomainProperty;
            var dtoProp = prop.DtoProperty;
            var sameType = SymbolEqualityComparer.IncludeNullability.Equals(domainProp,
                dtoProp);
            if (sameType) return new DirectMappedProperty(domainProp, dtoProp);

            //check for conversions
            var fromDomainConv = compilation.ClassifyCommonConversion(domainProp.Type, dtoProp.Type);
            var toDomainConv = compilation.ClassifyCommonConversion(dtoProp.Type, domainProp.Type);

            if (fromDomainConv.IsImplicit && toDomainConv.IsImplicit) return new DirectMappedProperty(domainProp, dtoProp);

            return null;
        }

        private ConverterMappedProperty? MapWithConverter(
            MatchedPropertyInfo prop
        )
        {
            string? converterName = BuiltInConverters.GetConverterName(
                prop.DomainProperty.Type,
                prop.DtoProperty.Type
            );

            if (converterName is not null)
            {
                INamedTypeSymbol converter = compilation.GetTypeByMetadataName(converterName)!;
                return new ConverterMappedProperty(
                    prop.DomainProperty,
                    prop.DtoProperty,
                    converter
                );
            }

            return null;
        }

        private DefaultMappedProperty? MapWithDefault(
            IPropertySymbol symbol,
            bool isDomain,
            string? defaultValue
        )
        {
            bool isNullable = symbol.Type.IsReferenceType
                ? _forceRefNullability || symbol.Type.NullableAnnotation == NullableAnnotation.Annotated
                : symbol.NullableAnnotation == NullableAnnotation.Annotated;

            if (isNullable)
            {
                return isDomain
                    ? new DefaultDomainMappedProperty(symbol, defaultValue ?? "default!")
                    : new DefaultDtoMappedProperty(symbol, defaultValue ?? "default!");
            }

            return null;
        }
    }
}