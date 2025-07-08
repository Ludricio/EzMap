using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringCharConverter : TypeConverter<string, char>
    {
        public override required ConverterSettings Settings { get; init; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(char right)
        {
            return right.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override char Convert(string? left)
        {
            if (left.Length == 1)
            {
                return left[0];
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(Char)}.");
        }
    }
}