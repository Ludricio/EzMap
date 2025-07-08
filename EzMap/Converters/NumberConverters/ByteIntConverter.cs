using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class ByteIntConverter : TypeConverter<byte, int>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte Convert(int right)
        {
            if (Settings.UseCheckedIntegerCasts)
                return checked((byte)right);
            return (byte)right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Convert(byte left)
        {
            return left;
        }
    }
}