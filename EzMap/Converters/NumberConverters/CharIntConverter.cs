using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class CharIntConverter : TypeConverter<char, int>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override char Convert(int right)
        {
            if (Settings.UseCheckedIntegerCasts)
                return checked((char)right);
            return (char)right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Convert(char left)
        {
            return left;
        }
    }
}