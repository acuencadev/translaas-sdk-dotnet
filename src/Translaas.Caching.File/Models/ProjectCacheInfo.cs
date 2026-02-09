using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Translaas.Caching.File.Models;

/// <summary>
/// Information about a cached project in the manifest.
/// </summary>
public class ProjectCacheInfo
{
    /// <summary>
    /// Gets or sets the list of cached language codes for this project.
    /// </summary>
    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; } = [];

    /// <summary>
    /// Gets or sets the timestamp of the last successful synchronization.
    /// </summary>
    [JsonPropertyName("lastSyncAt")]
    public DateTimeOffset? LastSyncAt { get; set; }

    /// <summary>
    /// Gets or sets the current synchronization status.
    /// </summary>
    [JsonPropertyName("status")]
    public CacheSyncStatus Status { get; set; }
}
