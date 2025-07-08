using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Diagnostics
{
    internal static class DiagnosticExtensions
    {
        public static void ReportDiagnostic(
            this SourceProductionContext spc,
            DiagnosticDescriptor descriptor,
            Location? location,
            params object[]? args
        )
        {
            var diagnostic = Diagnostic.Create(
                descriptor,
                location,
                args
            );
            spc.ReportDiagnostic(diagnostic);
        }
    }
}