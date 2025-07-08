using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using EzMap.Generators.Pipelines.DefaultSettings.Models;
using EzMap.Generators.Pipelines.Mappers.Converters;
using EzMap.Generators.Pipelines.Mappers.Models;
using EzMap.Generators.Pipelines.Mappers.Models.MappingInfos;
using EzMap.Generators.Pipelines.Mappers.PropertyMappers;

namespace EzMap.Generators.Pipelines.Mappers
{
    internal class MapperPipelineOutputProcessor(
        SourceProductionContext spc,
        DefaultSettingsPipelineData defaultSettings,
        Compilation compilation
    )
    {
        public void Process(ImmutableArray<MapperPipelineData> data)
        {
            var mapperInfos = data
                .Select(d => d.MapperInfo)
                .OfType<MapperInfo>()
                .ToList();

            //todo utilize to check for nested mapped relations.
            //if a mapped relation exists within an output mapper,
            //that mapped relation should be used for conversion unless an
            //explicit converter is provided.
            ImmutableDictionary<(ITypeSymbol, ITypeSymbol), MapperInfo> mapperCache =
                mapperInfos.ToImmutableDictionary(mapperInfo =>
                    (mapperInfo.DomainTypeSymbol, mapperInfo.DtoTypeSymbol));

            foreach (var mapperInfo in mapperInfos)
            {
                var finalizer = new PropertyMappingFinalizer(compilation, mapperInfo.AutoMapConvention);
                var mappings = finalizer.MapPropertiesFromPropertyInfos(mapperInfo.PropertyInfos);

                var converterInstances = ConverterInstanceDictionaryBuilder.BuildConverterInstanceDictionary(mappings);


                string generatedSource = MappingClassCreator.CreateMappingClass(
                    mapperInfo,
                    mappings,
                    converterInstances,
                    defaultSettings.DefaultSettingsSymbol!
                );

                spc.AddSource($"{mapperInfo.ClassSymbol.Name}.g.cs", SourceText.From(generatedSource, Encoding.UTF8));
            }
        }
    }
}