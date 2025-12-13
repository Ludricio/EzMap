namespace EzMap;

/// <summary>
/// Specifies how to handle nullable to non-nullable conversions.
/// </summary>
public enum NullableFallbackBehavior
{
    /// <summary>
    /// Use default(T) for the target type.
    /// </summary>
    Default = 0,
    
    /// <summary>
    /// Throw an exception when mapping null to non-nullable.
    /// </summary>
    Throw = 1,
    
    /// <summary>
    /// Emit a diagnostic warning only.
    /// </summary>
    Diagnostic = 2
}
