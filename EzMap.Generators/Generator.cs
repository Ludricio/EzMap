using EzMap.Generators.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics;
using EzMap.Generators.Pipelines.DefaultSettings;
using EzMap.Generators.Pipelines.DefaultSettings.Models;
using EzMap.Generators.Pipelines.Mappers;
using EzMap.Generators.Pipelines.Mappers.Converters;
using EzMap.Generators.Pipelines.Mappers.Models;

namespace EzMap.Generators;

[Generator]
public class MapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapperPipeline = new MapperPipeline();
        var mapperNonGenericProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                "EzMap.MapClassAttribute",
                predicate: mapperPipeline.Filter,
                transform: mapperPipeline.Transform)
            .WithTrackingName("MapperPipelineNonGeneric")
            .Collect();

        var mapperGenericProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                "EzMap.MapClassAttribute`2",
                predicate: mapperPipeline.Filter,
                transform: mapperPipeline.Transform)
            .WithTrackingName("MapperPipelineGeneric")
            .Collect();

        // var mapperProvider = mapperGenericProvider;
        var mapperProvider = mapperGenericProvider.Combine(mapperNonGenericProvider)
            .Select((d, _) =>
            {
                return d.Left.Concat(d.Right).ToImmutableArray();
            })
            .WithTrackingName("MapperPipelineCombined");


        var defaultSettingsPipeline = new DefaultSettingsPipeline();
        var defaultSettingsProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                NameConstants.DefaultConverterSettingsAttribute,
                predicate: defaultSettingsPipeline.Filter,
                transform: defaultSettingsPipeline.Transform
            )
            .WithTrackingName("DefaultConverterSettingsPipeline")
            .Collect();

        var compilationProvider = context.CompilationProvider
            .WithTrackingName("CompilationProvider");

        var combinedProvider = mapperProvider
            .Combine(defaultSettingsProvider)
            .Combine(compilationProvider)
            .Select((d, _) => new SourceOutputProvider
            {
                MapperPipelineOutput = d.Left.Left,
                DefaultSettingsPipelineOutput = d.Left.Right,
                Compilation = d.Right
            }).WithTrackingName("CombinedProvider");

        context.RegisterSourceOutput(combinedProvider, (spc, provider) =>
        {
            foreach (var diagnostic in provider.MapperPipelineOutput
                         .SelectMany(d => d.DiagnosticReport.Diagnostics))
            {
                spc.ReportDiagnostic(
                    diagnostic.Descriptor,
                    diagnostic.Location,
                    diagnostic.DescriptorArguments
                );
            }

            var compilation = provider.Compilation;
            BuiltInConverters.Init(compilation);
            DefaultSettingsPipelineProcessor defaultSettingsOutputProcessor = new(spc, compilation);
            DefaultSettingsPipelineData defaultSettings =
                defaultSettingsOutputProcessor.Process(provider.DefaultSettingsPipelineOutput);
            
            MapperPipelineOutputProcessor mapperOutputProcessor = new(spc, defaultSettings, compilation);
            mapperOutputProcessor.Process(provider.MapperPipelineOutput);
        });
    }
}

internal class SourceOutputProvider
{
    public required ImmutableArray<MapperPipelineData> MapperPipelineOutput { get; init; }
    public required ImmutableArray<DefaultSettingsPipelineData> DefaultSettingsPipelineOutput { get; init; }
    public required Compilation Compilation { get; init; }
}