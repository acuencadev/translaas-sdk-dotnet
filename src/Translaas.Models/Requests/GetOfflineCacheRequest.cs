using System.Text.Json.Serialization;

namespace Translaas.Models.Requests;

/// <summary>
/// Query parameters for the offline-cache ZIP endpoint.
/// </summary>
public sealed class GetOfflineCacheRequest
{
    /// <summary>
    /// Project identifier.
    /// </summary>
    [JsonPropertyName("project")]
    public string? Project { get; set; }

    /// <summary>
    /// Optional release channel.
    /// </summary>
    [JsonPropertyName("channel")]
    public string? Channel { get; set; }

    /// <summary>
    /// Optional snapshot / version (query <c>v</c>).
    /// </summary>
    [JsonPropertyName("v")]
    public string? Version { get; set; }

    /// <summary>
    /// When set, includes entry context in bundled JSON.
    /// </summary>
    [JsonPropertyName("includeContext")]
    public bool? IncludeContext { get; set; }
}
