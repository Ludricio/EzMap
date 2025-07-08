using EzMap.Generators.Diagnostics;
using EzMap.Generators.Pipelines.Mappers.Models.MappingInfos;
using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers
{
    internal class MapperArgumentExtractor(AttributeData mapAttr, DiagnosticReport diagnosticReport)
    {
        private SyntaxNode? _syntaxRef;

        public (ITypeSymbol?, ITypeSymbol?) ExtractConstructorArguments(CancellationToken ct)
        {
            ITypeSymbol? domainType = null;
            ITypeSymbol? dtoType = null;

            if (mapAttr.ConstructorArguments.Length == 2)
            {
                domainType = mapAttr.ConstructorArguments[0].Value as ITypeSymbol;
                dtoType = mapAttr.ConstructorArguments[1].Value as ITypeSymbol;
            }
            else if (mapAttr.AttributeClass?.TypeArguments.Length == 2)
            {
                domainType = mapAttr.AttributeClass!.TypeArguments[0];
                dtoType = mapAttr.AttributeClass.TypeArguments[1];
            }
            else
            {
                ct.ThrowIfCancellationRequested();
                _syntaxRef ??= mapAttr.ApplicationSyntaxReference?.GetSyntax();
                diagnosticReport.Register(
                    EzMapDiagnostic.MapPropertyAttributeInvalidArguments,
                    _syntaxRef?.GetLocation(),
                    mapAttr.AttributeClass!.ToDisplayString()
                );
            }

            bool isValid = true;
            isValid = ValidateIsClassOrStruct(domainType, ct) && isValid;
            isValid = ValidateIsClassOrStruct(dtoType, ct) && isValid;

            return isValid
                ? (domainType, dtoType)
                : (null, null);
        }

        public void ExtractNamedArguments(MapperInfo info, CancellationToken ct)
        {
            foreach (var prop in mapAttr.NamedArguments)
            {
                ct.ThrowIfCancellationRequested();
                switch (prop.Key)
                {
                    case nameof(MapClassAttribute.AutoMap):
                        info.AutoMap = (bool)prop.Value.Value!;
                        break;
                    case nameof(MapClassAttribute.AutoMapConvention):
                        info.AutoMapConvention = (AutoMapConvention?)prop.Value.Value ?? AutoMapConvention.Default;
                        break;
                    case nameof(MapClassAttribute.CustomToDomainName):
                        info.CustomToDomainName = prop.Value.Value as string;
                        break;
                    case nameof(MapClassAttribute.CustomToDtoName):
                        info.CustomToDtoName = prop.Value.Value as string;
                        break;
                    case nameof(MapClassAttribute.ConverterSettings):
                        info.ConverterSettings = prop.Value.Value as ITypeSymbol;
                        break;
                }
            }
        }

        private bool ValidateIsClassOrStruct(ITypeSymbol? type, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (type?.TypeKind is TypeKind.Class or TypeKind.Struct) return true;
            _syntaxRef ??= mapAttr.ApplicationSyntaxReference?.GetSyntax();
            diagnosticReport.Register(
                EzMapDiagnostic.MapPropertyAttributeArgumentIsNotClassOrStruct,
                _syntaxRef?.GetLocation(),
                type?.ToDisplayString() ?? "null"
            );
            return false;
        }
    }
}