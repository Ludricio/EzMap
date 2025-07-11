using EzMap.Generators.Pipelines.Mappers.Models.Mappings;
using EzMap.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Converters
{
    internal static class ConverterInstanceDictionaryBuilder
    {
        public static Dictionary<string, INamedTypeSymbol> BuildConverterInstanceDictionary(
            List<PropertyMapping> propertyMappings
        )
        {
            Dictionary<string, INamedTypeSymbol> converterInstances = new();

            foreach (var mapping in propertyMappings.OfType<ConverterMappedProperty>())
            {
                var instanceName = mapping.ConverterInstanceName;
                converterInstances.TryAdd(instanceName, mapping.TypeConverterSymbol);
            }

            return converterInstances;
        }
    }
}