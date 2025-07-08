using EzMap.Generators.Diagnostics;
using EzMap.Generators.Pipelines.Mappers.Models.MappingInfos;
using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.PropertyMappers
{
    internal static class PropertyInfoMapper
    {
        public static List<PropertyInfo> GetPropertyInfos(
            INamedTypeSymbol classSymbol,
            ITypeSymbol domainTypeSymbol,
            ITypeSymbol dtoTypeSymbol,
            AutoMapConvention autoMapConvention,
            bool autoMap,
            DiagnosticReport diagnosticReport,
            CancellationToken ct
        )
        {
            ct.ThrowIfCancellationRequested();
            List<PropertyInfo> explicitMappings =
            [
                ..ExplicitPropertyInfoParser.GetInfos(
                    classSymbol,
                    domainTypeSymbol,
                    dtoTypeSymbol,
                    diagnosticReport,
                    ct
                )
            ];
            if (!autoMap) return explicitMappings;

            ct.ThrowIfCancellationRequested();
            List<PropertyInfo> autoMappedProperties = PropertyInfoAutoMapParser.GetInfos(
                domainTypeSymbol,
                dtoTypeSymbol,
                explicitMappings,
                autoMapConvention,
                ct
            );

            ct.ThrowIfCancellationRequested();
            return
            [
                ..explicitMappings,
                ..autoMappedProperties
            ];
        }
    }
}