namespace Translaas.Models.Responses;

/// <summary>
/// Result of downloading the offline translation ZIP bundle.
/// </summary>
public sealed class OfflineCacheDownloadResult
{
    /// <summary>
    /// <see langword="true"/> when the server returned <c>304 Not Modified</c>.
    /// </summary>
    public bool NotModified { get; set; }

    /// <summary>
    /// Response weak ETag when present.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Suggested filename from <c>Content-Disposition</c>, when present.
    /// </summary>
    public string? SuggestedFileName { get; set; }

    /// <summary>
    /// ZIP bytes for <c>200 OK</c>; <see langword="null"/> when <see cref="NotModified"/> is <see langword="true"/>.
    /// </summary>
    public byte[]? Content { get; set; }
}
