using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Translaas.Models.Responses;

/// <summary>
/// Represents the available locales for a project.
/// </summary>
public class ProjectLocales
{
    /// <summary>
    /// Gets or sets the list of available locale codes (e.g., "en", "fr", "es").
    /// </summary>
    [JsonPropertyName("locales")]
    public List<string> Locales { get; set; } = [];
}
