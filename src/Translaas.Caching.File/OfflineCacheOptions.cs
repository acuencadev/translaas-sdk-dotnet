using System;
using System.Collections.Generic;

namespace Translaas.Caching.File;

/// <summary>
/// Configuration options for file-based offline caching.
/// </summary>
public class OfflineCacheOptions
{
    /// <summary>
    /// Default cache directory name.
    /// </summary>
    public const string DefaultCacheDirectory = ".translaas-cache";

    /// <summary>
    /// Gets or sets whether offline file caching is enabled.
    /// </summary>
    /// <remarks>
    /// When enabled, translations will be cached to local JSON files
    /// that persist across application restarts.
    /// </remarks>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the directory path for storing cache files.
    /// Can be absolute or relative to the application base directory.
    /// </summary>
    /// <remarks>
    /// Defaults to ".translaas-cache" in the application's base directory.
    /// The directory will be created if it does not exist.
    /// </remarks>
    public string CacheDirectory { get; set; } = DefaultCacheDirectory;

    /// <summary>
    /// Gets or sets the fallback behavior when offline cache is enabled.
    /// </summary>
    /// <remarks>
    /// Determines how the SDK behaves when both API and cache are available.
    /// Defaults to <see cref="OfflineFallbackMode.CacheFirst"/>.
    /// </remarks>
    public OfflineFallbackMode FallbackMode { get; set; } = OfflineFallbackMode.CacheFirst;

    /// <summary>
    /// Gets or sets whether to automatically sync cache when online.
    /// </summary>
    /// <remarks>
    /// When enabled, the cache will be periodically updated in the background
    /// based on <see cref="AutoSyncInterval"/>.
    /// </remarks>
    public bool AutoSync { get; set; } = true;

    /// <summary>
    /// Gets or sets the interval for automatic cache synchronization.
    /// Only applies when <see cref="AutoSync"/> is true.
    /// </summary>
    /// <remarks>
    /// Defaults to 1 hour. Set to null to disable interval-based sync.
    /// </remarks>
    public TimeSpan? AutoSyncInterval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets the list of project IDs to pre-cache.
    /// When specified, these projects will be automatically downloaded and cached.
    /// </summary>
    /// <remarks>
    /// If empty, no projects are pre-cached automatically.
    /// Projects will still be cached on first access.
    /// </remarks>
    public List<string> Projects { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of language codes to pre-cache.
    /// When specified, only these languages will be cached for each project.
    /// </summary>
    /// <remarks>
    /// If empty, all available languages are cached for each project.
    /// Language codes should match the project's available locales (e.g., "en", "es", "fr").
    /// </remarks>
    public List<string> Languages { get; set; } = [];

    /// <summary>
    /// Gets or sets the default project ID for single entry lookups.
    /// </summary>
    /// <remarks>
    /// Required when using offline caching with <see cref="Client.ITranslaasClient.GetEntryAsync"/>,
    /// as that method does not include a project parameter.
    /// Should be set to the first project in <see cref="Projects"/> if not explicitly specified.
    /// </remarks>
    public string? DefaultProjectId { get; set; }

    /// <summary>
    /// Gets or sets the hybrid caching options.
    /// </summary>
    /// <remarks>
    /// Hybrid caching combines in-memory caching (L1) with file caching (L2)
    /// for optimal performance and persistence.
    /// </remarks>
    public HybridCacheOptions HybridCache { get; set; } = new();
}
