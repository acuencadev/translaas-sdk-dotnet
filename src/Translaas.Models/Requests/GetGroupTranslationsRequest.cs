using System.Text.Json.Serialization;

namespace Translaas.Models.Requests;

/// <summary>
/// Request model for getting all translations for a group.
/// </summary>
public class GetGroupTranslationsRequest
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    [JsonPropertyName("project")]
    public string? Project { get; set; }

    /// <summary>
    /// Gets or sets the translation group name.
    /// </summary>
    [JsonPropertyName("group")]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the language code (e.g., "en", "fr").
    /// </summary>
    [JsonPropertyName("lang")]
    public string? Lang { get; set; }

    /// <summary>
    /// Gets or sets the optional format parameter.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the optional release channel.
    /// </summary>
    [JsonPropertyName("channel")]
    public string? Channel { get; set; }

    /// <summary>
    /// Gets or sets the optional snapshot / version (query <c>v</c>).
    /// </summary>
    [JsonPropertyName("v")]
    public string? Version { get; set; }

    /// <summary>
    /// When set, requests entry context in the payload.
    /// </summary>
    [JsonPropertyName("includeContext")]
    public bool? IncludeContext { get; set; }
}
