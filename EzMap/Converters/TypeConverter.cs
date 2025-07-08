using EzMap.Converters.Settings;

namespace EzMap.Converters;

public interface ITypeConverter
{
    public ConverterSettings Settings { get; init; }
    object? Convert(object source);
}

public interface ITypeConverter<TLeft, TRight> : ITypeConverter
{
    TLeft? Convert(TRight right);
    TRight? Convert(TLeft left);
}

public abstract class TypeConverter<TLeft, TRight> : ITypeConverter<TLeft, TRight>
{
    public abstract ConverterSettings Settings { get; init; }
    protected Type LeftType { get; } = typeof(TLeft);
    protected Type RightType { get; } = typeof(TRight);

    public object? Convert(object source)
    {
        if (source is TLeft left) return Convert(left);
        if (source is TRight right) return Convert(right);
        throw new ArgumentException(
            $"Expected source of type {LeftType.Name} or {RightType.Name}, but got {source.GetType().Name}.");
    }

    public abstract TLeft? Convert(TRight? right);

    public abstract TRight? Convert(TLeft? left);
}