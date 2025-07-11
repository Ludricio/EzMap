using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Models.MappingInfos;

internal class MapperInfo
{
    public required AttributeData AttributeData { get; set; }
    public required ISymbol ClassSymbol { get; set; }
    public required ITypeSymbol DomainTypeSymbol { get; set; }
    public required ITypeSymbol DtoTypeSymbol { get; set; }
    public List<PropertyInfo> PropertyInfos { get; set; } = [];
    public bool AutoMap { get; set; } = true;
    public AutoMapConvention AutoMapConvention { get; set; } = AutoMapConvention.Default;
    public string? CustomToDomainName { get; set; } 
    public string? CustomToDtoName { get; set; } 
    public ITypeSymbol? ConverterSettings { get; set; }
}