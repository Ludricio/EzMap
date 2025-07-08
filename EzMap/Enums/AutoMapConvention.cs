namespace EzMap;

[Flags]
public enum AutoMapConvention
{
    /// <summary>
    /// Default convention for auto-mapping.
    /// Uses following rules:
    /// <ul>
    /// <li><see cref="TryStripPrefixes"/></li>
    /// <li><see cref="TryStripSuffixes"/></li>
    /// <li><see cref="UseDefaultTypeConverters"/></li>
    /// </ul>
    /// </summary>
    Default = TryStripPrefixes | TryStripSuffixes | UseDefaultTypeConverters,

    /// <summary>
    /// Use case-sensitive mapping.
    /// </summary>
    CaseSensitive = 1 << 0,

    /// <summary>
    /// Try to strip common prefixes from property names when mapping.<br/>
    /// Common prefixes include:
    /// <ul>
    /// <li>get</li>
    /// <li>set</li>
    /// <li>is</li>
    /// <li>_</li>
    /// </ul>
    /// </summary>
    TryStripPrefixes = 1 << 1,

    /// <summary>
    /// Try to strip common suffixes from property names when mapping.<br/>
    /// Common suffixes include:
    /// <ul>
    /// <li>Dto</li>
    /// <li>Domain</li>
    /// <li>Model</li>
    /// <li>Entity</li>
    /// </ul>
    /// </summary>
    TryStripSuffixes = 1 << 2,

    /// <summary>
    /// Use default type converters for mapping properties.<br/>
    /// Default type converters include:
    /// <ul>
    /// <li><see cref="string"/> &lt;-&gt; <see cref="int"/></li>
    /// <li><see cref="string"/> &lt;-&gt; <see cref="double"/></li>
    /// <li><see cref="string"/> &lt;-&gt; <see cref="float"/></li>
    /// <li><see cref="string"/> &lt;-&gt; <see cref="decimal"/></li>
    /// <li><see cref="string"/> &lt;-&gt; <see cref="long"/></li>
    /// <li><see cref="string"/> &lt;-&gt; <see cref="short"/></li>
    /// <li><see cref="string"/> &lt;-&gt; <see cref="byte"/></li>
    /// <li><see cref="string"/> &lt;-&gt; <see cref="bool"/></li>
    /// <li><see cref="string"/> &lt;-&gt; <see cref="DateTime"/></li>
    /// <li><see cref="string"/> &lt;-&gt; <see cref="Guid"/></li>
    /// <li><see cref="string"/> &lt;-&gt; <see cref="Enum"/></li>
    /// <li><see cref="int"/> &lt;-&gt; <see cref="byte"/></li>
    /// <li><see cref="int"/> &lt;-&gt; <see cref="short"/></li>
    /// <li><see cref="int"/> &lt;-&gt; <see cref="long"/></li>
    /// <li><see cref="int"/> &lt;-&gt; <see cref="bool"/></li>
    /// <li><see cref="int"/> &lt;-&gt; <see cref="DateTime"/></li>
    /// </ul>
    /// 
    /// </summary>
    UseDefaultTypeConverters = 1 << 3,

    /// <summary>
    /// Enable forced nullability for reference types in auto-mapping.<br/>
    /// Including this flag will allow the mapper to set reference type properties to <see langword="null"/> even if
    /// they are within a nullable context but not annotated as nullable (<c>string?</c>)
    /// </summary>
    ForceReferenceNullability = 1 << 4,
    All = ~(-1 << 5)
}