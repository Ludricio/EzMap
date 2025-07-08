namespace EzMap.Generators
{
    public static class NameConstants
    {
        public static NameConstant MapClassAttribute { get; } =
            new(nameof(EzMap), nameof(EzMap.MapClassAttribute));
        
        public static NameConstant MapPropertyAttribute { get; }=
            new(nameof(EzMap), nameof(EzMap.MapPropertyAttribute));

        public static NameConstant DefaultConverterSettingsAttribute { get; } =
            new(nameof(EzMap), nameof(EzMap.DefaultConverterSettingsAttribute));
        
        public static NameConstant ConverterSettings { get; } =
            new("EzMap.Converters.Settings", nameof(Converters.Settings.ConverterSettings));
    }

    public readonly record struct NameConstant(string Namespace, string Name)
    {
        public string DisplayName { get; } = $"{Namespace}.{Name}";

        public static implicit operator string(NameConstant nameConstant) => nameConstant.DisplayName;

        public override string ToString() => DisplayName;
    }
}