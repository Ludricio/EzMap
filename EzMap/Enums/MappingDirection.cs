namespace EzMap;

/// <summary>
/// Specifies the direction of mapping generation.
/// </summary>
public enum MappingDirection
{
    /// <summary>
    /// Generate mappings in both directions (A &lt;-&gt; B).
    /// </summary>
    Both = 0,
    
    /// <summary>
    /// Generate mapping from source to target only (A -&gt; B).
    /// </summary>
    SourceToTarget = 1,
    
    /// <summary>
    /// Generate mapping from target to source only (A &lt;- B).
    /// </summary>
    TargetToSource = 2
}
