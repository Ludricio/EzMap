using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class CharByteConverter : TypeConverter<char, byte>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override char Convert(byte right)
        {
            return (char)right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte Convert(char left)
        {
            if (Settings.UseCheckedIntegerCasts)
                return checked((byte)left);
            return (byte)left;
        }
    }
}