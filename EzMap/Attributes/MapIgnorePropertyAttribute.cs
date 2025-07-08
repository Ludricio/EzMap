namespace EzMap;

[AttributeUsage(AttributeTargets.Class)]
public class MapIgnorePropertyAttribute(Type type, string propertyName) : Attribute
{
    public Type Type { get; } = type;
    public string PropertyName { get; } = propertyName;
}
    
[AttributeUsage(AttributeTargets.Class)]
public class MapIgnorePropertyAttribute<TType>(string propertyName) : MapIgnorePropertyAttribute(typeof(TType), propertyName);