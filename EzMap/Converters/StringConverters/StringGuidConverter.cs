using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class StringGuidConverter : TypeConverter<string, Guid>
    {
        public override required ConverterSettings Settings { get; init; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string Convert(Guid right)
        {
            return right.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Guid Convert(string? left)
        {
            if (Guid.TryParse(left, out Guid result))
            {
                return result;
            }

            throw new FormatException($"Cannot convert '{left}' to {nameof(Guid)}.");
        }
    }
}