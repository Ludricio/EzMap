using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringFloatConverter : TypeConverter<string, float>
    {
        public override required ConverterSettings Settings { get; init; }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(float right)
        {
            return right.ToString(Settings.Float.FormatProvider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float Convert(string? left)
        {
            var (numberStyles, provider) = Settings.Float;
            if (float.TryParse(left, numberStyles, provider, out float result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(Single)}.");
        }
    }
}