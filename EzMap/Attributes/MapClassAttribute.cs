using EzMap.Converters.Settings;

namespace EzMap;

[AttributeUsage(AttributeTargets.Class)]
public class MapClassAttribute(Type domainType, Type dtoType) : Attribute
{
    public Type DomainType { get; } = domainType;
    public Type DtoType { get; } = dtoType;
    public bool AutoMap { get; init; } = true;
    public AutoMapConvention  AutoMapConvention { get; init; } = AutoMapConvention.Default;
    public string? CustomToDomainName { get; init; }
    public string? CustomToDtoName { get; init; }
    public ConverterSettings? ConverterSettings { get; init; }
}

[AttributeUsage(AttributeTargets.Class)]
public class MapClassAttribute<TDomain, TDto>() : MapClassAttribute(typeof(TDomain), typeof(TDto))
    where TDomain : class
    where TDto : class;
