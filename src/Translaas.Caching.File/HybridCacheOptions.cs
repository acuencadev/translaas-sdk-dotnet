using System;

namespace Translaas.Caching.File;

/// <summary>
/// Configuration options for hybrid caching (memory L1 + file L2).
/// </summary>
public class HybridCacheOptions
{
    /// <summary>
    /// Gets or sets whether hybrid caching is enabled.
    /// </summary>
    /// <remarks>
    /// When enabled, translations are cached in both memory (L1) and file (L2).
    /// This provides fast lookups with persistence.
    /// </remarks>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the expiration time for memory cache (L1) entries.
    /// </summary>
    /// <remarks>
    /// If null, memory cache entries never expire based on time (only evicted when memory limit is reached).
    /// Defaults to 30 minutes.
    /// </remarks>
    public TimeSpan? MemoryCacheExpiration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets the maximum number of entries in the memory cache (L1).
    /// </summary>
    /// <remarks>
    /// If null, no limit is enforced. When the limit is exceeded, oldest entries are evicted.
    /// Defaults to 1000 entries.
    /// </remarks>
    public int? MaxMemoryCacheEntries { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether to warm up the L1 cache from L2 on startup.
    /// </summary>
    /// <remarks>
    /// When enabled, the memory cache is populated from the file cache when the application starts.
    /// This improves first-request performance but increases startup time.
    /// </remarks>
    public bool WarmupOnStartup { get; set; } = false;
}
