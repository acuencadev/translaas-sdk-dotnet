using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Translaas.Models.Responses;

/// <summary>
/// Represents the available locales for a project.
/// </summary>
public class ProjectLocales
{
    /// <summary>
    /// Gets or sets the project identifier when returned by the API.
    /// </summary>
    [JsonPropertyName("project")]
    public string? Project { get; set; }

    /// <summary>
    /// Gets or sets the list of available locale codes (e.g., "en", "fr", "es").
    /// </summary>
    [JsonPropertyName("locales")]
    public List<string> Locales { get; set; } = [];

    /// <summary>
    /// Gets or sets locale metadata last-modified timestamp when returned by the API.
    /// </summary>
    [JsonPropertyName("lastModifiedUtc")]
    public DateTimeOffset? LastModifiedUtc { get; set; }
}
