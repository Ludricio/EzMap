using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class ShortByteConverter : TypeConverter<short, byte>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override short Convert(byte right)
        {
            return right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte Convert(short left)
        {
            if (Settings.UseCheckedIntegerCasts)
                return checked((byte)left);
            return (byte)left;
        }
    }
}