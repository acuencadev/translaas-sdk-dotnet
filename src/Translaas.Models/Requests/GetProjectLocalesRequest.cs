using System.Text.Json.Serialization;

namespace Translaas.Models.Requests;

/// <summary>
/// Request model for getting available locales for a project.
/// </summary>
public class GetProjectLocalesRequest
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    [JsonPropertyName("project")]
    public string? Project { get; set; }
}
