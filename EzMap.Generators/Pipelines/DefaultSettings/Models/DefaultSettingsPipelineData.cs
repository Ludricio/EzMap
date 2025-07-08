using EzMap.Generators.Diagnostics;
using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.DefaultSettings.Models
{
    internal class DefaultSettingsPipelineData : IPipelineData
    {
        public INamedTypeSymbol? DefaultSettingsSymbol { get; set; }
        public DiagnosticReport DiagnosticReport { get; } = new();
    }
}