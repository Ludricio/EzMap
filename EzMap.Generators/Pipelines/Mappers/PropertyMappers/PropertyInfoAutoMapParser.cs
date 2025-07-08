using EzMap.Generators.Pipelines.Mappers.Models.MappingInfos;
using Microsoft.CodeAnalysis;

namespace EzMap.Generators.Pipelines.Mappers.PropertyMappers
{
    internal static class PropertyInfoAutoMapParser
    {
        public static List<PropertyInfo> GetInfos(
            ITypeSymbol domainTypeSymbol,
            ITypeSymbol dtoTypeSymbol,
            List<PropertyInfo> matchedInfos,
            AutoMapConvention autoMapConvention,
            CancellationToken ct
        )
        {
            ct.ThrowIfCancellationRequested();
            HashSet<string> matchedDomainProps = [];
            HashSet<string> matchedDtoProps = [];

            foreach (var prop in matchedInfos.OfType<MatchedPropertyInfo>())
            {
                matchedDomainProps.Add(prop.DomainProperty.Name);
                matchedDtoProps.Add(prop.DtoProperty.Name);
            }

            ct.ThrowIfCancellationRequested();

            // Get unmatched properties from the domain type
            var unmatchedDomainProps = domainTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !matchedDomainProps.Contains(p.Name))
                .ToList();

            ct.ThrowIfCancellationRequested();

            // Get unmatched properties from the DTO type
            var unmatchedDtoProps = dtoTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !matchedDtoProps.Contains(p.Name))
                .ToList();

            ct.ThrowIfCancellationRequested();
            // If there are no unmatched properties, return early
            if (unmatchedDomainProps.Count == 0 && unmatchedDtoProps.Count == 0)
                return [];

            PropertyMatchResult matchResult = MatchProperties(
                unmatchedDomainProps,
                unmatchedDtoProps,
                autoMapConvention
            );


            ct.ThrowIfCancellationRequested();
            var matchedPropertyInfos = CreatePropertyInfos(matchResult.MatchedProperties);

            ct.ThrowIfCancellationRequested();
            var unmatchedPropertyInfos = CreatePropertyInfos(
                matchResult.UnmatchedDomainProperties,
                matchResult.UnmatchedDtoProperties,
                autoMapConvention
            );
            return
            [
                ..matchedPropertyInfos,
                ..unmatchedPropertyInfos
            ];
        }

        private static PropertyMatchResult MatchProperties(
            List<IPropertySymbol> domainProps,
            List<IPropertySymbol> dtopProps,
            AutoMapConvention autoMapConvention
        )
        {
            PropertyMatchResult result = new();
            HashSet<string> matchedDtoNames = new(StringComparer.Ordinal);
            //match properties based on names, using the specified AutoMapConvention
            foreach (IPropertySymbol domainProp in domainProps)
            {
                // Find a matching target property based on the naming convention
                IPropertySymbol? match = dtopProps.FirstOrDefault(dtopProp =>
                    NamesMatch(domainProp.Name, dtopProp.Name, autoMapConvention));

                if (match != null)
                {
                    result.MatchedProperties.Add((domainProp, match));
                    matchedDtoNames.Add(match.Name);
                }
                else
                {
                    result.UnmatchedDomainProperties.Add(domainProp);
                }
            }

            // Find unmatched DTO properties
            foreach (IPropertySymbol dtopProp in dtopProps)
            {
                // If the DTO property was not matched, add it to the unmatched list
                if (!matchedDtoNames.Contains(dtopProp.Name))
                    result.UnmatchedDtoProperties.Add(dtopProp);
            }

            return result;
        }

        private static List<MatchedPropertyInfo> CreatePropertyInfos(
            List<(IPropertySymbol, IPropertySymbol)> matchedProperties
        )
        {
            var matched = matchedProperties
                .Select(pair =>
                {
                    var (domainProp, dtoProp) = pair;
                    return new MatchedPropertyInfo { DomainProperty = domainProp, DtoProperty = dtoProp };
                })
                .ToList();
            return matched;
        }

        private static bool IsNullable(IPropertySymbol prop, bool refNullabilityForced)
        {
            if (prop.Type.IsReferenceType)
            {
                return refNullabilityForced || prop.NullableAnnotation == NullableAnnotation.Annotated;
            }

            return prop.NullableAnnotation == NullableAnnotation.Annotated;
        }

        private static List<UnmatchedPropertyInfo> CreatePropertyInfos(
            List<IPropertySymbol> unmatchedDomainProperties,
            List<IPropertySymbol> unmatchedDtoProperties,
            AutoMapConvention autoMapConvention
        )
        {
            //we need to create UnmatchedPropertyMappingInfo for each unmatched property
            //for each one, we need to check for nullability, if it's nullable we can use the default value
            bool refNullabilityForced = autoMapConvention.HasFlag(AutoMapConvention.ForceReferenceNullability);

            List<UnmatchedPropertyInfo> unmatchedPropInfos = [];
            unmatchedPropInfos.AddRange(
                from domainProp in unmatchedDomainProperties
                let isNullable = IsNullable(domainProp, refNullabilityForced)
                select new UnmatchedPropertyInfo
                {
                    DomainProperty = domainProp,
                    DtoProperty = null,
                    DefaultValue = isNullable
                        ? "default!"
                        : null
                });

            unmatchedPropInfos.AddRange(
                from dtoProp in unmatchedDtoProperties
                let isNullable = IsNullable(dtoProp, refNullabilityForced)
                select new UnmatchedPropertyInfo
                {
                    DomainProperty = null,
                    DtoProperty = dtoProp,
                    DefaultValue = isNullable
                        ? "default!"
                        : null
                });
            return unmatchedPropInfos;
        }

        private class PropertyMatchResult
        {
            public List<(IPropertySymbol, IPropertySymbol)> MatchedProperties { get; } = [];
            public List<IPropertySymbol> UnmatchedDomainProperties { get; } = [];
            public List<IPropertySymbol> UnmatchedDtoProperties { get; } = [];
        }

        private static bool NamesMatch(string source, string target, AutoMapConvention autoMapConvention)
        {
            StringComparison comparison = autoMapConvention.HasFlag(AutoMapConvention.CaseSensitive)
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            //match by direct name equality
            if (string.Equals(source, target, comparison))
                return true;

            //try all variations of prefix/suffix stripping and compare for each variation if a match is found 
            string strippedSource = source;
            string strippedTarget = target;
            if (autoMapConvention.HasFlag(AutoMapConvention.TryStripPrefixes))
            {
                strippedSource = PropertyStripper.StripPrefixes(strippedSource);
                strippedTarget = PropertyStripper.StripPrefixes(strippedTarget);
            }

            if (autoMapConvention.HasFlag(AutoMapConvention.TryStripSuffixes))
            {
                strippedSource = PropertyStripper.StripSuffixes(strippedSource);
                strippedTarget = PropertyStripper.StripSuffixes(strippedTarget);
            }

            return string.Equals(strippedSource, strippedTarget, comparison);
        }
    }

    internal static class PropertyStripper
    {
        private static readonly string[] _prefixes = ["get", "set", "is", "_"];
        private static readonly string[] _suffixes = ["Dto", "Domain", "Entity", "Model"];

        public static string StripPrefixes(string name)
        {
            foreach (string prefix in _prefixes)
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return name.Substring(prefix.Length);
            return name;
        }

        public static string StripSuffixes(string name)
        {
            foreach (string suffix in _suffixes)
                if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    return name.Substring(0, name.Length - suffix.Length);
            return name;
        }
    }
}