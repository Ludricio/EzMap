using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class ShortIntConverter : TypeConverter<short, int>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override short Convert(int right)
        {
            if (Settings.UseCheckedIntegerCasts)
                return checked((short)right);
            return (short)right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Convert(short left)
        {
            return left;
        }
    }
}