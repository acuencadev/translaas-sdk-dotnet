using System.Text.Json.Serialization;

namespace Translaas.Models.Requests;

/// <summary>
/// Request model for getting a single translation entry.
/// </summary>
public class GetTranslationRequest
{
    /// <summary>
    /// Gets or sets the translation group name.
    /// </summary>
    [JsonPropertyName("group")]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the translation entry key.
    /// </summary>
    [JsonPropertyName("entry")]
    public string? Entry { get; set; }

    /// <summary>
    /// Gets or sets the language code (e.g., "en", "fr").
    /// </summary>
    [JsonPropertyName("lang")]
    public string? Lang { get; set; }

    /// <summary>
    /// Gets or sets the optional number for pluralization.
    /// Supports both integer and decimal/fractional numbers (e.g., 1.31).
    /// </summary>
    [JsonPropertyName("n")]
    public decimal? Number { get; set; }

    /// <summary>
    /// Gets or sets the project identifier when the API key is not scoped to a single project.
    /// </summary>
    [JsonPropertyName("project")]
    public string? Project { get; set; }

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
}
