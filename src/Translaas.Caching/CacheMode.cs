namespace Translaas.Caching;

/// <summary>
/// Specifies the caching mode for Translaas translation data.
/// </summary>
public enum CacheMode
{
    /// <summary>
    /// No caching is performed. All requests go directly to the API.
    /// </summary>
    None = 0,

    /// <summary>
    /// Cache individual translation entries.
    /// </summary>
    Entry = 1,

    /// <summary>
    /// Cache entire translation groups.
    /// </summary>
    Group = 2,

    /// <summary>
    /// Cache entire translation projects.
    /// </summary>
    Project = 3
}
