using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace EzMap.Converters.Settings
{
    public class ConverterSettings
    {
        public virtual bool UseCheckedIntegerCasts => false;
        public virtual FloatingPointSettings Float { get; } = new();
        public virtual FloatingPointSettings Double { get; } = new();
        public virtual FloatingPointSettings Decimal { get; } = new();
        public virtual DateTimeSettings DateTime { get; } = new();
        public virtual DateTimeSettings DateTimeOffset { get; } = new();

        public class FloatingPointSettings(
            NumberStyles numberStyles = NumberStyles.Float | NumberStyles.AllowThousands,
            IFormatProvider? formatProvider = null)
        {
            public NumberStyles NumberStyles => numberStyles;
            public IFormatProvider FormatProvider => formatProvider ?? CultureInfo.CurrentCulture;

            public void Deconstruct(out NumberStyles styles, out IFormatProvider provider)
            {
                styles = NumberStyles;
                provider = FormatProvider;
            }
        }

        public class DateTimeSettings(
            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
            string formatString = "O",
            DateTimeStyles dateTimeStyles = DateTimeStyles.None,
            IFormatProvider? formatProvider = null
        )
        {
            public string Format { get; } = formatString;
            public DateTimeStyles DateTimeStyles { get; } = DateTimeStyles.None;
            public IFormatProvider FormatProvider { get; } = formatProvider ?? CultureInfo.CurrentCulture;

            public void Deconstruct(out string format, out DateTimeStyles styles, out IFormatProvider provider)
            {
                format = Format;
                styles = DateTimeStyles;
                provider = FormatProvider;
            }
        }
    }
}