using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringDecimalConverter : TypeConverter<string, decimal>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(decimal right)
        {
            return right.ToString(Settings.Float.FormatProvider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override decimal Convert(string? left)
        {
            var (numberStyles, provider) = Settings.Float;
            if (decimal.TryParse(left, numberStyles, provider, out decimal result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(Decimal)}.");
        }
    }
}