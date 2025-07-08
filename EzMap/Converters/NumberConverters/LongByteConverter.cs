using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class LongByteConverter : TypeConverter<long, byte>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long Convert(byte right)
        {
            return right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte Convert(long left)
        {
            if (Settings.UseCheckedIntegerCasts)
                return checked((byte)left);
            return (byte)left;
        }
    }
}