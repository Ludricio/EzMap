using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using EzMap.Generators.Diagnostics;
using EzMap.Generators.Pipelines.Mappers.Models;
using EzMap.Generators.Pipelines.Mappers.Models.MappingInfos;
using EzMap.Generators.Pipelines.Mappers.PropertyMappers;

namespace EzMap.Generators.Pipelines.Mappers
{
    internal sealed class MapperPipeline : IPipeline<MapperPipelineData>
    {
        public bool Filter(SyntaxNode syntaxNode, CancellationToken _) =>
            syntaxNode is ClassDeclarationSyntax
            {
                AttributeLists.Count: > 0,
            } clazz &&
            clazz.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
            && clazz.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
            && !clazz.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword))
            && !clazz.Modifiers.Any(m => m.IsKind(SyntaxKind.FileKeyword));

        public MapperPipelineData Transform(GeneratorAttributeSyntaxContext context,
            CancellationToken ct)
        {
            MapperPipelineData data = new();
            if (!VerifySymbol(context, data.DiagnosticReport, out INamedTypeSymbol? symbol))
            {
                return data;
            }

            ct.ThrowIfCancellationRequested();
            AttributeData mapAttr = context.Attributes[0];
            if (mapAttr.AttributeClass is null)
            {
                data.DiagnosticReport.Register(
                    EzMapDiagnostic.MapClassAttributeNotFound,
                    context.TargetNode.GetLocation(),
                    symbol.Name
                );
                return data;
            }
            
            ct.ThrowIfCancellationRequested();

            var argumentExtractor = new MapperArgumentExtractor(mapAttr, data.DiagnosticReport);
            var (domainType, dtoType) = argumentExtractor.ExtractConstructorArguments(ct);
            if (domainType is null || dtoType is null)
            {
                return data;
            }

            ct.ThrowIfCancellationRequested();

            data.MapperInfo = new MapperInfo
            {
                AttributeData = mapAttr,
                ClassSymbol = symbol,
                DomainTypeSymbol = domainType,
                DtoTypeSymbol = dtoType
            };
            
            argumentExtractor.ExtractNamedArguments(data.MapperInfo, ct);

            data.MapperInfo.PropertyInfos = PropertyInfoMapper.GetPropertyInfos(
                symbol,
                domainType,
                dtoType,
                data.MapperInfo.AutoMapConvention,
                data.MapperInfo.AutoMap,
                data.DiagnosticReport,
                ct
            );
            return data;
        }

        private static bool VerifySymbol(
            GeneratorAttributeSyntaxContext context,
            DiagnosticReport diagnosticReport,
            [NotNullWhen(true)] out INamedTypeSymbol? symbol
        )
        {
            if (context.TargetSymbol is not INamedTypeSymbol classSymbol ||
                classSymbol is { IsReferenceType: false, IsValueType: false } ||
                classSymbol.TypeKind == TypeKind.Interface ||
                classSymbol.TypeKind == TypeKind.Enum ||
                classSymbol.TypeKind == TypeKind.Delegate
               )
            {
                diagnosticReport.Register(
                    EzMapDiagnostic.MapClassAttributeTargetNotClassOrStruct,
                    context.TargetNode.GetLocation()
                );
                symbol = null;
                return false;
            }

            symbol = classSymbol;
            return true;
        }
    }
}