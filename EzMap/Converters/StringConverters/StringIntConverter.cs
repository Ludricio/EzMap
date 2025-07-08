using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringIntConverter : TypeConverter<string, int>
    {
        public override required ConverterSettings Settings { get; init; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(int right)
        {
            return right.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Convert(string? left)
        {
            if (int.TryParse(left, out int result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(Int32)}.");
        }
    }
}