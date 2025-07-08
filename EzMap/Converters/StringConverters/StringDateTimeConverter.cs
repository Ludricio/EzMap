using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringDateTimeConverter : TypeConverter<string, DateTime>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(DateTime right)
        {
            var (formatString, _, provider) = Settings.DateTime;
            return right.ToString(formatString, provider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override DateTime Convert(string? left)
        {
            var (_, dateTimeStyles, provider) = Settings.DateTime;
            if (DateTime.TryParse(left, provider, dateTimeStyles, out DateTime result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(DateTime)}.");
        }
    }
}