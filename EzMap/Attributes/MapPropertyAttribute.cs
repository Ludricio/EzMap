
namespace EzMap;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MapPropertyAttribute(string domainPropName, string dtoPropName) : Attribute
{
    public string DomainPropName { get; } = domainPropName;
    public string DtoPropName { get; } = dtoPropName;

    public Type? TypeConverter { get; init; }
}