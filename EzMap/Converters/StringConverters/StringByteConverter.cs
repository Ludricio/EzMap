using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringByteConverter : TypeConverter<string, byte>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(byte right)
        {
            return right.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte Convert(string? left)
        {
            if (byte.TryParse(left, out byte result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(Byte)}.");
        }
    }
}