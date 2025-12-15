using System;
using System.Text.Json.Serialization;

using Translaas.Models.Responses;

namespace Translaas.Caching.File.Models;

/// <summary>
/// Represents a cached translation project with metadata.
/// Stored as project.json in the language-specific directory.
/// </summary>
public class CachedProject
{
    /// <summary>
    /// Gets or sets the timestamp when the project was cached.
    /// </summary>
    [JsonPropertyName("cachedAt")]
    public DateTimeOffset CachedAt { get; set; }

    /// <summary>
    /// Gets or sets the optional expiration timestamp.
    /// </summary>
    /// <remarks>
    /// If null, the cache never expires based on time alone.
    /// </remarks>
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the cached translation project data.
    /// </summary>
    [JsonPropertyName("data")]
    public TranslationProject? Data { get; set; }
}
