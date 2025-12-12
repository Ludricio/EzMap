namespace EzMap;

/// <summary>
/// Specifies the constructor selection strategy for creating target instances.
/// </summary>
public enum ConstructorSelectionStrategy
{
    /// <summary>
    /// Use the parameterless constructor (default).
    /// </summary>
    Parameterless = 0,
    
    /// <summary>
    /// Use the constructor with the most parameters.
    /// </summary>
    Greediest = 1,
    
    /// <summary>
    /// Use the constructor marked with [MapConstructor] attribute.
    /// </summary>
    Annotated = 2
}
