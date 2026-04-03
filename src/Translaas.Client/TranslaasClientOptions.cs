using System;
using System.Text.RegularExpressions;

using Translaas.Caching;
using Translaas.Models.Errors;

namespace Translaas.Client;

/// <summary>
/// Configuration options for the Translaas client.
/// </summary>
public class TranslaasClientOptions
{
    // Note: Base URL should be the API host only (e.g. https://api.example.com). The client appends /sdk/v1/translations/... and /api/v1/... paths.
    private const string DefaultBaseUrl = "https://sdk-api.translaas.local";
    private static readonly Regex UrlRegex = new(@"^https?://", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets or sets the API key for authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the Translaas API.
    /// </summary>
    public string BaseUrl { get; set; } = DefaultBaseUrl;

    /// <summary>
    /// Gets or sets the timeout for HTTP requests.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets the caching mode for translation data.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="CacheMode.None"/> if not specified.
    /// </remarks>
    public CacheMode CacheMode { get; set; } = CacheMode.None;

    /// <summary>
    /// Gets or sets the absolute expiration time for cached items.
    /// </summary>
    /// <remarks>
    /// If specified, cached items will expire after this duration, regardless of activity.
    /// </remarks>
    public TimeSpan? CacheAbsoluteExpiration { get; set; }

    /// <summary>
    /// Gets or sets the sliding expiration time for cached items.
    /// </summary>
    /// <remarks>
    /// If specified, cached items will expire if not accessed within this duration.
    /// It will not extend beyond the absolute expiration.
    /// </remarks>
    public TimeSpan? CacheSlidingExpiration { get; set; }

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <param name="skipApiValidation">If true, skips validation of ApiKey and BaseUrl. Used when offline cache is enabled with CacheOnly mode.</param>
    /// <exception cref="TranslaasConfigurationException">Thrown when validation fails.</exception>
    public void Validate(bool skipApiValidation = false)
    {
        if (!skipApiValidation)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new TranslaasConfigurationException("ApiKey is required and cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(BaseUrl))
            {
                throw new TranslaasConfigurationException("BaseUrl is required and cannot be null or empty.");
            }

            if (!UrlRegex.IsMatch(BaseUrl))
            {
                throw new TranslaasConfigurationException($"BaseUrl must be a valid HTTP or HTTPS URL. Provided value: {BaseUrl}");
            }
        }

        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
        {
            throw new TranslaasConfigurationException($"Timeout must be greater than zero. Provided value: {Timeout.Value}");
        }
    }
}
