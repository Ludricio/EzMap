using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class ShortCharConverter : TypeConverter<short, char>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override short Convert(char right)
        {
            return (short)right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override char Convert(short left)
        {
            if (Settings.UseCheckedIntegerCasts)
                return checked((char)left);
            return (char)left;
        }
    }
}