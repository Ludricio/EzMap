using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringShortConverter : TypeConverter<string, short>
    {
        public override required ConverterSettings Settings { get; init; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(short right)
        {
            return right.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override short Convert(string? left)
        {
            if (short.TryParse(left, out short result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(Int16)}.");
        }
    }
}