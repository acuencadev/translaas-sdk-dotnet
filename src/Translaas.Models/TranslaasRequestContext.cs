namespace Translaas.Models;

/// <summary>
/// Optional per-request options for SDK translation calls (channel/version scoping, project for text, conditional GET).
/// Response fields are populated by the HTTP client on success.
/// </summary>
public sealed class TranslaasRequestContext
{
    /// <summary>
    /// Optional release channel (query <c>channel</c>).
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// Optional snapshot / version (query <c>v</c>).
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// For <c>GET /sdk/v1/translations/text</c> when the API key is not scoped to a single project (query <c>project</c>).
    /// </summary>
    public string? Project { get; set; }

    /// <summary>
    /// Include entry context in nested payloads (query <c>includeContext</c>). Used by group, project, and offline-cache.
    /// </summary>
    public bool? IncludeContext { get; set; }

    /// <summary>
    /// When set, sends <c>If-None-Match</c> for weak ETag conditional requests.
    /// </summary>
    public string? IfNoneMatch { get; set; }

    /// <summary>
    /// After the request completes, the response <c>ETag</c> when present.
    /// </summary>
    public string? ResponseEtag { get; set; }

    /// <summary>
    /// After the request completes, <see langword="true"/> when the server returned <c>304 Not Modified</c>.
    /// </summary>
    public bool NotModified { get; set; }
}
