using System.Text.Json.Serialization;

namespace Translaas.Models.Errors;

/// <summary>
/// Represents an error response from the Translaas API.
/// </summary>
public class TranslaasError
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the error code, if available.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
