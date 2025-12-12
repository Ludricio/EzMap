namespace EzMap;

/// <summary>
/// Specifies that mapping methods should be generated for the two types.
/// Place this attribute on a static partial class to generate mapping extension methods.
/// </summary>
/// <typeparam name="TSource">The first type to map.</typeparam>
/// <typeparam name="TTarget">The second type to map.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class MapAttribute<TSource, TTarget> : Attribute
{
    /// <summary>
    /// Gets or sets whether to generate instance extension methods (e.g., source.MapToTarget()).
    /// Default is true.
    /// </summary>
    public bool GenerateInstanceExtensions { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate static extension members using new C# extension syntax (e.g., TSource.From(target)).
    /// Default is false (feature detection applies).
    /// </summary>
    public bool GenerateStaticExtensions { get; set; } = false;
}
