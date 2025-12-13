using System;

namespace EzMap;

/// <summary>
/// Marks a constructor to be used for mapping when ConstructorSelectionStrategy is set to Annotated.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
public sealed class MapConstructorAttribute : Attribute
{
}
