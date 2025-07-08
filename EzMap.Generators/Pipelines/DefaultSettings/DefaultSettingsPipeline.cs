using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using EzMap.Generators.Diagnostics;
using EzMap.Generators.Pipelines.DefaultSettings.Models;

namespace EzMap.Generators.Pipelines.DefaultSettings
{
    internal class DefaultSettingsPipeline : IPipeline<DefaultSettingsPipelineData>
    {
        public bool Filter(SyntaxNode syntaxNode, CancellationToken _) =>
            syntaxNode is ClassDeclarationSyntax clazz &&
            !clazz.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword))
            && !clazz.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
            && !clazz.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword))
            && !clazz.Modifiers.Any(m => m.IsKind(SyntaxKind.FileKeyword));

        public DefaultSettingsPipelineData Transform(GeneratorAttributeSyntaxContext context, CancellationToken ct)
        {
            DefaultSettingsPipelineData data = new();

            if (!VerifySymbol(context, data.DiagnosticReport, out INamedTypeSymbol? symbol))
            {
                return data;
            }

            ct.ThrowIfCancellationRequested();

            var hasDefaultCtor = symbol.InstanceConstructors.Any(ctor => ctor is
            {
                Parameters.Length: 0,
                DeclaredAccessibility: Accessibility.Public or Accessibility.Internal,
            });

            if (!hasDefaultCtor)
            {
                data.DiagnosticReport.Register(
                    EzMapDiagnostic.DefaultConverterSettingsMissingDefaultConstructor,
                    symbol.Locations.FirstOrDefault() ?? context.TargetNode.GetLocation(),
                    symbol.Name
                );
                return data;
            }

            data.DefaultSettingsSymbol = symbol;
            return data;
        }

        private static bool VerifySymbol(
            GeneratorAttributeSyntaxContext context,
            DiagnosticReport diagnosticReport,
            [NotNullWhen(true)] out INamedTypeSymbol? symbol
        )
        {
            if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
            {
                diagnosticReport.Register(
                    EzMapDiagnostic.DefaultConverterSettingsTargetNotClass,
                    context.TargetNode.GetLocation()
                );
                symbol = null;
                return false;
            }

            if (classSymbol.IsGenericType)
            {
                diagnosticReport.Register(
                    EzMapDiagnostic.DefaultConverterSettingsGenericTypeNotSupported,
                    classSymbol.Locations.FirstOrDefault() ?? context.TargetNode.GetLocation(),
                    classSymbol.Name
                );
                symbol = null;
                return false;
            }

            if (classSymbol.IsAbstract)
            {
                diagnosticReport.Register(
                    EzMapDiagnostic.DefaultConverterSettingsAbstractClassNotSupported,
                    classSymbol.Locations.FirstOrDefault() ?? context.TargetNode.GetLocation(),
                    classSymbol.Name
                );
                symbol = null;
                return false;
            }

            if (classSymbol.IsStatic)
            {
                diagnosticReport.Register(
                    EzMapDiagnostic.DefaultConverterSettingsStaticClassNotSupported,
                    classSymbol.Locations.FirstOrDefault() ?? context.TargetNode.GetLocation(),
                    classSymbol.Name
                );
                symbol = null;
                return false;
            }

            if (!(classSymbol.DeclaredAccessibility == Accessibility.Public ||
                  classSymbol.DeclaredAccessibility == Accessibility.Internal))
            {
                diagnosticReport.Register(
                    EzMapDiagnostic.DefaultConverterSettingsInvalidAccessibility,
                    classSymbol.Locations.FirstOrDefault() ?? context.TargetNode.GetLocation(),
                    classSymbol.Name
                );
                symbol = null;
                return false;
            }

            symbol = classSymbol;
            return true;
        }
    }
}