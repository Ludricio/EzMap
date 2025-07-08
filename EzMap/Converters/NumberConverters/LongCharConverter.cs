using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class LongCharConverter : TypeConverter<long, char>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long Convert(char right)
        {
            return right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override char Convert(long left)
        {
            if (Settings.UseCheckedIntegerCasts)
                return checked((char)left);
            return (char)left;
        }
    }
}