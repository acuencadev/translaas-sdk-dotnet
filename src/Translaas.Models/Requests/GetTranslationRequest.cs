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
}
