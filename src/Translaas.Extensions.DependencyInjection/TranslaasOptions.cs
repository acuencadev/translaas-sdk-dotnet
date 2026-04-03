using System;
using System.ComponentModel.DataAnnotations;

using Translaas.Caching;
using Translaas.Caching.File;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Configuration options for the Translaas SDK dependency injection integration.
/// </summary>
public class TranslaasOptions
{
    private const string DefaultBaseUrl = "https://sdk-api.translaas.local";

    /// <summary>
    /// Gets or sets the API key for authentication.
    /// </summary>
    /// <remarks>
    /// This property is required when offline cache is not enabled or when using fallback modes other than CacheOnly.
    /// When <see cref="OfflineCache.Enabled"/> is true and <see cref="OfflineCacheOptions.FallbackMode"/> is <see cref="OfflineFallbackMode.CacheOnly"/>,
    /// this property is optional as the API will never be called.
    /// </remarks>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the Translaas API.
    /// </summary>
    /// <remarks>
    /// Defaults to "https://sdk-api.translaas.local/api" if not specified.
    /// Must be a valid HTTP or HTTPS URL when provided.
    /// This property is required when offline cache is not enabled or when using fallback modes other than CacheOnly.
    /// When <see cref="OfflineCache.Enabled"/> is true and <see cref="OfflineCacheOptions.FallbackMode"/> is <see cref="OfflineFallbackMode.CacheOnly"/>,
    /// this property is optional as the API will never be called.
    /// </remarks>
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

    /// <summary>
    /// Gets or sets the offline caching options.
    /// </summary>
    /// <remarks>
    /// Configure this to enable file-based offline caching for translation data.
    /// When enabled, translations are cached locally in JSON files for offline use.
    /// </remarks>
    public OfflineCacheOptions OfflineCache { get; set; } = new();

    /// <summary>
    /// Gets or sets the default language code used when no other provider returns a value.
    /// </summary>
    /// <remarks>
    /// This property is used by <see cref="DefaultLanguageProvider"/> as a fallback
    /// when no other language provider can determine the language.
    /// </remarks>
    /// <example>
    /// <code>
    /// options.DefaultLanguage = LanguageCodes.English;
    /// </code>
    /// </example>
    public string? DefaultLanguage { get; set; }
}
