using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class LongShortConverter : TypeConverter<long, short>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long Convert(short right)
        {
            return right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override short Convert(long left)
        {
            if (Settings.UseCheckedIntegerCasts)
                return checked((short)left);
            return (short)left;
        }
    }
}