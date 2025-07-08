using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringDoubleConverter : TypeConverter<string, double>
    {
        public override required ConverterSettings Settings { get; init; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(double right)
        {
            return right.ToString(Settings.Double.FormatProvider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override double Convert(string? left)
        {
            var (numberStyles, provider) = Settings.Double;
            if (double.TryParse(left, numberStyles, provider, out double result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(Double)}.");
        }
    }
}