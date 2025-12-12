namespace EzMap.Generators.Utils;

/// <summary>
/// Provides utilities for normalizing property and type names for matching purposes.
/// </summary>
internal static class NameNormalizer
{
    private static readonly string[] Suffixes = new[] { "Dto", "Entity", "Model", "ViewModel" };

    /// <summary>
    /// Normalizes a property or type name by removing common prefixes and suffixes.
    /// </summary>
    /// <param name="name">The name to normalize.</param>
    /// <returns>The normalized name.</returns>
    public static string Normalize(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // Remove leading underscores
        var normalized = name.TrimStart('_');

        // Remove common suffixes (case-insensitive)
        foreach (var suffix in Suffixes)
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
    /// <returns>True if the normalized names match (case-insensitive).</returns>
    public static bool NormalizedEquals(string name1, string name2)
    {
        var normalized1 = Normalize(name1);
        var normalized2 = Normalize(name2);
        return string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase);
    }
}
