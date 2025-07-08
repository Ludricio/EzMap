using EzMap.Converters;
using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.Converters
{
    internal static class BuiltInConverters
    {
        private const string EmNs = $"{nameof(EzMap)}.{nameof(EzMap.Converters)}";

        private static Dictionary<(string, string), string> _converters = null!;

        public static string? GetConverterName(ITypeSymbol fromType, ITypeSymbol toType)
        {
            string fromTypeName = fromType.ToDisplayString();
            string toTypeName = toType.ToDisplayString();
            if (_converters.TryGetValue((fromTypeName, toTypeName), out string converterName))
                return converterName;
            if (_converters.TryGetValue((toTypeName, fromTypeName), out converterName))
                return converterName;
            return null;
        }

        public static void Init(Compilation compilation)
        {
            string intName = compilation.GetSpecialType(SpecialType.System_Int32).ToDisplayString();
            string stringName = compilation.GetSpecialType(SpecialType.System_String).ToDisplayString();
            string boolName = compilation.GetSpecialType(SpecialType.System_Boolean).ToDisplayString();
            string dateTimeName = compilation.GetSpecialType(SpecialType.System_DateTime).ToDisplayString();
            const string guidName = $"{nameof(System)}.{nameof(Guid)}";
            const string dateTimeOffsetName = $"{nameof(System)}.{nameof(DateTimeOffset)}";
            string decimalName = compilation.GetSpecialType(SpecialType.System_Decimal).ToDisplayString();
            string doubleName = compilation.GetSpecialType(SpecialType.System_Double).ToDisplayString();
            string floatName = compilation.GetSpecialType(SpecialType.System_Single).ToDisplayString();
            string longName = compilation.GetSpecialType(SpecialType.System_Int64).ToDisplayString();
            string shortName = compilation.GetSpecialType(SpecialType.System_Int16).ToDisplayString();
            string byteName = compilation.GetSpecialType(SpecialType.System_Byte).ToDisplayString();
            string charName = compilation.GetSpecialType(SpecialType.System_Char).ToDisplayString();

            _converters = new Dictionary<(string, string), string>
            {
                { (intName, stringName), $"{EmNs}.{nameof(StringIntConverter)}" },
                { (intName, longName), $"{EmNs}.{nameof(LongIntConverter)}" },
                { (intName, shortName), $"{EmNs}.{nameof(ShortIntConverter)}" },
                { (intName, byteName), $"{EmNs}.{nameof(ByteIntConverter)}" },
                { (intName, charName), $"{EmNs}.{nameof(CharIntConverter)}" },
                { (stringName, intName), $"{EmNs}.{nameof(StringIntConverter)}" },
                { (stringName, longName), $"{EmNs}.{nameof(StringLongConverter)}" },
                { (stringName, shortName), $"{EmNs}.{nameof(StringShortConverter)}" },
                { (stringName, byteName), $"{EmNs}.{nameof(StringByteConverter)}" },
                { (stringName, dateTimeName), $"{EmNs}.{nameof(StringDateTimeConverter)}" },
                { (stringName, decimalName), $"{EmNs}.{nameof(StringDecimalConverter)}" },
                { (stringName, doubleName), $"{EmNs}.{nameof(StringDoubleConverter)}" },
                { (stringName, floatName), $"{EmNs}.{nameof(StringFloatConverter)}" },
                { (stringName, boolName), $"{EmNs}.{nameof(StringBoolConverter)}" },
                { (stringName, charName), $"{EmNs}.{nameof(StringCharConverter)}" },
                { (stringName, guidName), $"{EmNs}.{nameof(StringGuidConverter)}" },
                { (stringName, dateTimeOffsetName), $"{EmNs}.{nameof(StringDateTimeOffsetConverter)}" },
                { (longName, shortName), $"{EmNs}.{nameof(LongShortConverter)}" },
                { (longName, byteName), $"{EmNs}.{nameof(LongByteConverter)}" },
                { (longName, charName), $"{EmNs}.{nameof(LongCharConverter)}" },
                { (shortName, byteName), $"{EmNs}.{nameof(ShortByteConverter)}" },
                { (shortName, charName), $"{EmNs}.{nameof(ShortCharConverter)}" },
                { (charName, byteName), $"{EmNs}.{nameof(CharByteConverter)}" },
            };
        }
    }
}