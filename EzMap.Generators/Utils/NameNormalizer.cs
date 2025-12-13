using System.Collections.Immutable;

namespace EzMap.Generators.Utils;

/// <summary>
/// Provides utilities for normalizing property and type names for matching purposes.
/// </summary>
internal static class NameNormalizer
{
    private static readonly ImmutableArray<string> DefaultPrefixes = ImmutableArray.Create("_");
    private static readonly ImmutableArray<string> DefaultSuffixes = ImmutableArray.Create("Dto", "Entity", "Model", "ViewModel");

    /// <summary>
    /// Normalizes a property or type name by removing common prefixes and suffixes.
    /// </summary>
    /// <param name="name">The name to normalize.</param>
    /// <param name="prefixes">Custom prefixes to remove. If empty, uses underscore.</param>
    /// <param name="suffixes">Custom suffixes to remove. If empty, uses default suffixes.</param>
    /// <returns>The normalized name.</returns>
    public static string Normalize(string name, ImmutableArray<string> prefixes, ImmutableArray<string> suffixes)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var normalized = name;

        // Remove prefixes
        var prefixesToUse = prefixes.IsDefaultOrEmpty ? DefaultPrefixes : prefixes;
        foreach (var prefix in prefixesToUse)
        {
            if (normalized.StartsWith(prefix))
            {
                normalized = normalized.Substring(prefix.Length);
                break; // Only remove one prefix
            }
        }

        // Remove suffixes
        var suffixesToUse = suffixes.IsDefaultOrEmpty ? DefaultSuffixes : suffixes;
        foreach (var suffix in suffixesToUse)
        {
            if (normalized.Length > suffix.Length && 
                normalized.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(0, normalized.Length - suffix.Length);
                break; // Only remove one suffix
            }
        }

        return normalized;
    }

    /// <summary>
    /// Checks if two names match after normalization.
    /// </summary>
    /// <param name="name1">First name to compare.</param>
    /// <param name="name2">Second name to compare.</param>
    /// <param name="prefixes">Custom prefixes to remove.</param>
    /// <param name="suffixes">Custom suffixes to remove.</param>
    /// <param name="caseInsensitive">Whether to use case-insensitive comparison.</param>
    /// <returns>True if the normalized names match.</returns>
    public static bool NormalizedEquals(
        string name1, 
        string name2, 
        ImmutableArray<string> prefixes, 
        ImmutableArray<string> suffixes,
        bool caseInsensitive)
    {
        var normalized1 = Normalize(name1, prefixes, suffixes);
        var normalized2 = Normalize(name2, prefixes, suffixes);
        
        var comparison = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(normalized1, normalized2, comparison);
    }
}
