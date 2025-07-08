using System.Runtime.CompilerServices;
using EzMap.Converters.Settings;

namespace EzMap.Converters
{
    public class LongIntConverter : TypeConverter<long, int>
    {
        public override required ConverterSettings Settings { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long Convert(int right)
        {
            return right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Convert(long left)
        {
            if (Settings.UseCheckedIntegerCasts)
                return checked((int)left);
            return (int)left;
        }
    }
}