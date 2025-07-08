using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringBoolConverter : TypeConverter<string, bool>
    {
        public override required ConverterSettings Settings { get; init; }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(bool right)
        {
            return right.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Convert(string? left)
        {
            if (bool.TryParse(left, out bool result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(Boolean)}.");
        }

    }
}