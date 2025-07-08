using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringLongConverter : TypeConverter<string, long>
    {
        public override required ConverterSettings Settings { get; init; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(long right)
        {
            return right.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long Convert(string? left)
        {
            if (long.TryParse(left, out long result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(Int64)}.");
        }
    }
}