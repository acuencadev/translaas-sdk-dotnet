using System;
using System.ComponentModel.DataAnnotations;

using Translaas.Caching;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Configuration options for the Translaas SDK dependency injection integration.
/// </summary>
public class TranslaasOptions
{
    private const string DefaultBaseUrl = "https://sdkapi.translaas.local/api";

    /// <summary>
    /// Gets or sets the API key for authentication.
    /// </summary>
    /// <remarks>
    /// This property is required and must not be null or empty.
    /// </remarks>
    [Required(ErrorMessage = "ApiKey is required and cannot be null or empty.")]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the Translaas API.
    /// </summary>
    /// <remarks>
    /// Defaults to "https://sdkapi.translaas.local/api" if not specified.
    /// Must be a valid HTTP or HTTPS URL.
    /// </remarks>
    [Required(ErrorMessage = "BaseUrl is required and cannot be null or empty.")]
    [Url(ErrorMessage = "BaseUrl must be a valid HTTP or HTTPS URL.")]
    public string BaseUrl { get; set; } = DefaultBaseUrl;

    /// <summary>
    /// Gets or sets the caching mode for translation data.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="CacheMode.None"/> if not specified.
    /// </remarks>
    public CacheMode CacheMode { get; set; } = CacheMode.None;

    /// <summary>
    /// Gets or sets the timeout for HTTP requests.
    /// </summary>
    /// <remarks>
    /// If not specified, the default HttpClient timeout will be used.
    /// Must be greater than zero if specified.
    /// </remarks>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets the absolute expiration time for cached items.
    /// </summary>
    /// <remarks>
    /// If specified, cached items will expire after this duration regardless of access.
    /// Only used when <see cref="CacheMode"/> is not <see cref="CacheMode.None"/>.
    /// </remarks>
    public TimeSpan? CacheAbsoluteExpiration { get; set; }

    /// <summary>
    /// Gets or sets the sliding expiration time for cached items.
    /// </summary>
    /// <remarks>
    /// If specified, cached items will expire after this duration of inactivity.
    /// Only used when <see cref="CacheMode"/> is not <see cref="CacheMode.None"/>.
    /// </remarks>
    public TimeSpan? CacheSlidingExpiration { get; set; }
}
