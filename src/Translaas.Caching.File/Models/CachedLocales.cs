using System;
using System.Text.Json.Serialization;

using Translaas.Models.Responses;

namespace Translaas.Caching.File.Models;

/// <summary>
/// Represents cached project locales with metadata.
/// Stored as locales.json in the project directory.
/// </summary>
public class CachedLocales
{
    /// <summary>
    /// Gets or sets the timestamp when the locales were cached.
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
    /// Gets or sets the cached project locales data.
    /// </summary>
    [JsonPropertyName("data")]
    public ProjectLocales? Data { get; set; }
}
