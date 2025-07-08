using EzMap.Generators.Diagnostics;
using EzMap.Generators.Pipelines.Mappers.Models.MappingInfos;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EzMap.Generators.Pipelines.Mappers.PropertyMappers
{
    internal static class ExplicitPropertyInfoParser
    {
        public static IEnumerable<ExplicitPropertyInfo> GetInfos(
            INamedTypeSymbol classSymbol,
            ITypeSymbol domainType,
            ITypeSymbol dtoType,
            DiagnosticReport diagnosticReport,
            CancellationToken ct
        )
        {
            IEnumerable<AttributeData> mappingAttributes = classSymbol
                .GetAttributes()
                .Where(attr => attr.AttributeClass?.ToDisplayString() == NameConstants.MapPropertyAttribute);

            foreach (AttributeData? attrData in mappingAttributes)
            {
                ct.ThrowIfCancellationRequested();
                var domainProp = GetPropertySymbol(attrData, domainType, true, diagnosticReport, ct);
                var dtoProp = GetPropertySymbol(attrData, dtoType, false, diagnosticReport, ct);

                ct.ThrowIfCancellationRequested();
                if (domainProp is null || dtoProp is null)
                {
                    // If either property is not found, skip this mapping
                    continue;
                }

                INamedTypeSymbol? converter = attrData.NamedArguments
                    .FirstOrDefault(kvp => kvp.Key == nameof(MapPropertyAttribute.TypeConverter))
                    .Value.Value as INamedTypeSymbol;

                yield return new ExplicitPropertyInfo
                {
                    DomainProperty = domainProp,
                    DtoProperty = dtoProp,
                    TypeConverter = converter,
                    TypeConverterName = converter?.ToDisplayString()
                };
            }
        }

        private static IPropertySymbol? GetPropertySymbol(
            AttributeData attrData,
            ITypeSymbol owningSymbol,
            bool isDomain,
            DiagnosticReport diagnosticReport,
            CancellationToken ct
        )
        {
            var index = isDomain ? 0 : 1;
            var propName = attrData.ConstructorArguments[index].Value!.ToString()!;
            var propSymbol = owningSymbol.GetMembers(propName)
                .OfType<IPropertySymbol>()
                .FirstOrDefault();
            if (propSymbol is null)
            {
                diagnosticReport.Register(EzMapDiagnostic.MissingPropertyOnMappedClass,
                    attrData.ApplicationSyntaxReference?.GetSyntax(ct).GetLocation(),
                    propName,
                    owningSymbol.ToDisplayString()
                );
                return null;
            }

            bool isValid = true;
            if (propSymbol.IsStatic)
            {
                diagnosticReport.Register(EzMapDiagnostic.MappedPropertyIsStatic,
                    attrData.ApplicationSyntaxReference?.GetSyntax(ct).GetLocation(),
                    propName,
                    owningSymbol.ToDisplayString()
                );
                isValid = false;
            }

            if (propSymbol.IsReadOnly)
            {
                diagnosticReport.Register(EzMapDiagnostic.MappedPropertyIsReadOnly,
                    attrData.ApplicationSyntaxReference?.GetSyntax(ct).GetLocation(),
                    propName,
                    owningSymbol.ToDisplayString()
                );
                isValid = false;
            }

            if (propSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
            {
                diagnosticReport.Register(EzMapDiagnostic.MappedPropertyIsUnaccessible,
                    attrData.ApplicationSyntaxReference?.GetSyntax(ct).GetLocation(),
                    propName,
                    owningSymbol.ToDisplayString()
                );
                isValid = false;
            }

            return isValid ? propSymbol : null;
        }
    }
}