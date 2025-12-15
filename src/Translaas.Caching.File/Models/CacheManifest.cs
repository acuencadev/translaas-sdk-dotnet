using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Translaas.Caching.File.Models;

/// <summary>
/// Represents metadata about the offline cache.
/// Stored as manifest.json in the cache directory root.
/// </summary>
public class CacheManifest
{
    /// <summary>
    /// Current manifest format version.
    /// </summary>
    public const string CurrentVersion = "1.0";

    /// <summary>
    /// Gets or sets the manifest format version.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = CurrentVersion;

    /// <summary>
    /// Gets or sets the SDK version that created/last updated the cache.
    /// </summary>
    [JsonPropertyName("sdkVersion")]
    public string SdkVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the cache was first created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last synchronization attempt.
    /// </summary>
    [JsonPropertyName("lastSyncAt")]
    public DateTimeOffset? LastSyncAt { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of cached projects, keyed by project ID.
    /// </summary>
    [JsonPropertyName("projects")]
    public Dictionary<string, ProjectCacheInfo> Projects { get; set; } = new();
}
