using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringDateTimeOffsetConverter : TypeConverter<string, DateTimeOffset>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(DateTimeOffset right)
        {
            var (formatString, _, provider) = Settings.DateTimeOffset;
            return right.ToString(formatString, provider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override DateTimeOffset Convert(string? left)
        {
            var (_, dateTimeStyles, provider) = Settings.DateTimeOffset;
            if (DateTimeOffset.TryParse(left, provider, dateTimeStyles, out DateTimeOffset result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(DateTimeOffset)}.");
        }
    }
}