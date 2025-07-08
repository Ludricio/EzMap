using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using EzMap.Generators.Diagnostics;
using EzMap.Generators.Pipelines.DefaultSettings.Models;

namespace EzMap.Generators.Pipelines.DefaultSettings
{
    internal class DefaultSettingsPipelineProcessor(SourceProductionContext spc, Compilation compilation)
    {
        public DefaultSettingsPipelineData Process(
            ImmutableArray<DefaultSettingsPipelineData> data
        )
        {
            if (data.IsDefaultOrEmpty)
            {
                var result = new DefaultSettingsPipelineData
                {
                    DefaultSettingsSymbol = compilation.GetTypeByMetadataName(NameConstants.ConverterSettings)!
                };
                return result;
            }

            if (data.Length > 1)
            {
                spc.ReportDiagnostic(
                    EzMapDiagnostic.MultipleDefaultConverterSettingsFound,
                    Location.None
                );
            }
            var defaultSettingsData = data[0];
            return defaultSettingsData;
        }
    }
}