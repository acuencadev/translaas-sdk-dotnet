namespace Translaas.Caching.File;

/// <summary>
/// Specifies the behavior when both online API and offline cache are available.
/// </summary>
public enum OfflineFallbackMode
{
    /// <summary>
    /// Always try cache first, fall back to API on cache miss.
    /// Best for performance when cache is likely to be warm.
    /// </summary>
    CacheFirst = 0,

    /// <summary>
    /// Always try API first, fall back to cache on API failure.
    /// Ensures freshest data when online.
    /// </summary>
    ApiFirst = 1,

    /// <summary>
    /// Use cache only, never call API (true offline mode).
    /// Useful when network is known to be unavailable.
    /// </summary>
    CacheOnly = 2,

    /// <summary>
    /// Use API only, but update cache in background.
    /// Cache serves as backup for future offline use.
    /// </summary>
    ApiOnlyWithBackup = 3
}
