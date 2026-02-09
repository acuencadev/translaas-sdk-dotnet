using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Translaas.Caching;
using Translaas.Models.Errors;
using Translaas.Models.Requests;
using Translaas.Models.Responses;

namespace Translaas.Client;

/// <summary>
/// Client implementation for interacting with the Translaas Translation Delivery API.
/// </summary>
public class TranslaasClient : ITranslaasClient
{
    private readonly HttpClient _httpClient;
    private readonly TranslaasClientOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITranslaasCacheProvider? _cacheProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="options">The client options.</param>
    /// <param name="cacheProvider">Optional cache provider for caching translation data.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient or options is null.</exception>
    /// <exception cref="TranslaasConfigurationException">Thrown when options validation fails.</exception>
    public TranslaasClient(HttpClient httpClient, TranslaasClientOptions options, ITranslaasCacheProvider? cacheProvider = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _cacheProvider = cacheProvider;
        
        // Skip API validation if ApiKey/BaseUrl are empty (used in CacheOnly offline mode)
        var skipApiValidation = string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.BaseUrl);
        _options.Validate(skipApiValidation);
        
        // Apply timeout from options if specified
        if (_options.Timeout.HasValue)
        {
            _httpClient.Timeout = _options.Timeout.Value;
        }
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc />
    public async Task<string> GetEntryAsync(
        string group,
        string entry,
        string lang,
        decimal? number = null,
        Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        // Parameter validation
        if (string.IsNullOrEmpty(group))
        {
            throw new ArgumentNullException(nameof(group));
        }

        if (string.IsNullOrEmpty(entry))
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (string.IsNullOrEmpty(lang))
        {
            throw new ArgumentNullException(nameof(lang));
        }

        // Merge number parameter into parameters dictionary
        // If N already exists in parameters, it takes precedence
        var mergedParameters = MergeNumberIntoParameters(number, parameters);

        // Check cache if caching is enabled for entries
        if (_cacheProvider != null && _options.CacheMode == CacheMode.Entry)
        {
            var cacheKey = CacheKeyBuilder.BuildEntryKey(group, entry, lang, number, mergedParameters);
            if (_cacheProvider.TryGetValue<string>(cacheKey, out var cachedValue))
            {
                if (cachedValue != null)
                {
                    return cachedValue;
                }
                // If cachedValue is null, fall through to fetch from API
            }
        }

        // Build request model
        var requestModel = new GetTranslationRequest
        {
            Group = group,
            Entry = entry,
            Lang = lang,
            Number = number
        };

        // Create HTTP request with parameters
        var request = BuildGetRequest("/api/translations/text", requestModel, mergedParameters);

        try
        {
            // Send request
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Handle 204 No Content: Return the entry key as fallback (common i18n pattern)
            // Note: 204 is a success status code, so check it before the error handling
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return entry;
            }

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            // Parse raw text response
            var result = await ParseTextResponse(response, cancellationToken).ConfigureAwait(false);

            // Store in cache if caching is enabled for entries
            if (_cacheProvider != null && _options.CacheMode == CacheMode.Entry)
            {
                var cacheKey = CacheKeyBuilder.BuildEntryKey(group, entry, lang, number, mergedParameters);
                _cacheProvider.Set(cacheKey, result, _options.CacheAbsoluteExpiration, _options.CacheSlidingExpiration);
            }

            return result;
        }
        catch (TranslaasApiException)
        {
            // Re-throw API exceptions as-is
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // This is a timeout, not a cancellation
            throw new TranslaasApiException(
                $"Request timed out after {_httpClient.Timeout.TotalSeconds} seconds.",
                HttpStatusCode.RequestTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new TranslaasApiException(
                $"Failed to retrieve translation: {ex.Message}",
                HttpStatusCode.BadRequest,
                ex);
        }
    }

    /// <inheritdoc />
    public async Task<Translaas.Models.Responses.TranslationGroup> GetGroupAsync(
        string project,
        string group,
        string lang,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        // Parameter validation
        if (string.IsNullOrEmpty(project))
        {
            throw new ArgumentNullException(nameof(project));
        }

        if (string.IsNullOrEmpty(group))
        {
            throw new ArgumentNullException(nameof(group));
        }

        if (string.IsNullOrEmpty(lang))
        {
            throw new ArgumentNullException(nameof(lang));
        }

        // Check cache if caching is enabled for groups or projects
        if (_cacheProvider != null && (_options.CacheMode == CacheMode.Group || _options.CacheMode == CacheMode.Project))
        {
            var cacheKey = CacheKeyBuilder.BuildGroupKey(project, group, lang, format);
            if (_cacheProvider.TryGetValue<TranslationGroup>(cacheKey, out var cachedValue))
            {
                if (cachedValue != null)
                {
                    return cachedValue;
                }
            }
        }

        // Build request model
        var requestModel = new GetGroupTranslationsRequest
        {
            Project = project,
            Group = group,
            Lang = lang,
            Format = format
        };

        // Create HTTP request
        var request = BuildGetRequest("/api/translations/group", requestModel);

        try
        {
            // Send request
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Handle 204 No Content: Return empty group
            // Note: 204 is a success status code, so check it before the error handling
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new TranslationGroup();
            }

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            // Deserialize JSON response
            var result = await ParseJsonResponse<TranslationGroup>(response, cancellationToken).ConfigureAwait(false);

            // Store in cache if caching is enabled for groups or projects
            if (_cacheProvider != null && (_options.CacheMode == CacheMode.Group || _options.CacheMode == CacheMode.Project))
            {
                var cacheKey = CacheKeyBuilder.BuildGroupKey(project, group, lang, format);
                _cacheProvider.Set(cacheKey, result, _options.CacheAbsoluteExpiration, _options.CacheSlidingExpiration);
            }

            return result;
        }
        catch (TranslaasApiException)
        {
            // Re-throw API exceptions as-is
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // This is a timeout, not a cancellation
            throw new TranslaasApiException(
                $"Request timed out after {_httpClient.Timeout.TotalSeconds} seconds.",
                HttpStatusCode.RequestTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new TranslaasApiException(
                $"Failed to retrieve translation group: {ex.Message}",
                HttpStatusCode.BadRequest,
                ex);
        }
    }

    /// <inheritdoc />
    public async Task<TranslationProject> GetProjectAsync(
        string project,
        string lang,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        // Parameter validation
        if (string.IsNullOrEmpty(project))
        {
            throw new ArgumentNullException(nameof(project));
        }

        if (string.IsNullOrEmpty(lang))
        {
            throw new ArgumentNullException(nameof(lang));
        }

        // Check cache if caching is enabled for projects
        if (_cacheProvider != null && _options.CacheMode == CacheMode.Project)
        {
            var cacheKey = CacheKeyBuilder.BuildProjectKey(project, lang, format);
            if (_cacheProvider.TryGetValue<TranslationProject>(cacheKey, out var cachedValue))
            {
                if (cachedValue != null)
                {
                    return cachedValue;
                }
                // If cachedValue is null, treat as cache miss and continue to fetch from API
            }
        }

        // Build request model
        var requestModel = new GetProjectTranslationsRequest
        {
            Project = project,
            Lang = lang,
            Format = format
        };

        // Create HTTP request
        var request = BuildGetRequest("/api/translations/project", requestModel);

        try
        {
            // Send request
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Handle 204 No Content: Return empty project
            // Note: 204 is a success status code, so check it before the error handling
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new TranslationProject();
            }

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            // Deserialize JSON response
            var result = await ParseJsonResponse<TranslationProject>(response, cancellationToken).ConfigureAwait(false);

            // Store in cache if caching is enabled for projects
            if (_cacheProvider != null && _options.CacheMode == CacheMode.Project)
            {
                var cacheKey = CacheKeyBuilder.BuildProjectKey(project, lang, format);
                _cacheProvider.Set(cacheKey, result, _options.CacheAbsoluteExpiration, _options.CacheSlidingExpiration);
            }

            return result;
        }
        catch (TranslaasApiException)
        {
            // Re-throw API exceptions as-is
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // This is a timeout, not a cancellation
            throw new TranslaasApiException(
                $"Request timed out after {_httpClient.Timeout.TotalSeconds} seconds.",
                HttpStatusCode.RequestTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new TranslaasApiException(
                $"Failed to retrieve translation project: {ex.Message}",
                HttpStatusCode.BadRequest,
                ex);
        }
    }

    /// <inheritdoc />
    public async Task<ProjectLocales> GetProjectLocalesAsync(
        string project,
        CancellationToken cancellationToken = default)
    {
        // Parameter validation
        if (string.IsNullOrEmpty(project))
        {
            throw new ArgumentNullException(nameof(project));
        }

        // Check cache if caching is enabled (locales can be cached with any cache mode)
        if (_cacheProvider != null && _options.CacheMode != CacheMode.None)
        {
            var cacheKey = CacheKeyBuilder.BuildLocalesKey(project);
            if (_cacheProvider.TryGetValue<ProjectLocales>(cacheKey, out var cachedValue))
            {
                if (cachedValue != null)
                {
                    return cachedValue;
                }
                // If cachedValue is null, fall through to fetch from API
            }
        }

        // Build request model
        var requestModel = new GetProjectLocalesRequest
        {
            Project = project
        };

        // Create HTTP request
        var request = BuildGetRequest("/api/translations/locales", requestModel);

        try
        {
            // Send request
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Handle 204 No Content: Return empty locales list
            // Note: 204 is a success status code, so check it before the error handling
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new ProjectLocales { Locales = [] };
            }

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            // Deserialize JSON response
            var result = await ParseJsonResponse<ProjectLocales>(response, cancellationToken).ConfigureAwait(false);

            // Store in cache if caching is enabled (locales can be cached with any cache mode)
            if (_cacheProvider != null && _options.CacheMode != CacheMode.None)
            {
                var cacheKey = CacheKeyBuilder.BuildLocalesKey(project);
                _cacheProvider.Set(cacheKey, result, _options.CacheAbsoluteExpiration, _options.CacheSlidingExpiration);
            }

            return result;
        }
        catch (TranslaasApiException)
        {
            // Re-throw API exceptions as-is
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // This is a timeout, not a cancellation
            throw new TranslaasApiException(
                $"Request timed out after {_httpClient.Timeout.TotalSeconds} seconds.",
                HttpStatusCode.RequestTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new TranslaasApiException(
                $"Failed to retrieve project locales: {ex.Message}",
                HttpStatusCode.BadRequest,
                ex);
        }
    }

    /// <summary>
    /// Parses a text response from the API.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response content as a string.</returns>
    private async Task<string> ParseTextResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

#if NETSTANDARD2_0
        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        var result = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif

        // Return empty string if response is null or empty
        return result ?? string.Empty;
    }

    /// <summary>
    /// Parses a JSON response from the API and deserializes it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="TranslaasApiException">Thrown when deserialization fails or returns null.</exception>
    private async Task<T> ParseJsonResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken) where T : class
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        // Read response content
#if NETSTANDARD2_0
        var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif

        // Deserialize JSON response
        T? result;
        try
        {
            result = JsonSerializer.Deserialize<T>(jsonContent, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new TranslaasApiException(
                $"Failed to deserialize response: {ex.Message}",
                response.StatusCode,
                ex,
                responseContent: jsonContent);
        }

        if (result == null)
        {
            throw new TranslaasApiException(
                "Failed to deserialize response from API.",
                response.StatusCode,
                responseContent: jsonContent);
        }

        return result;
    }

    /// <summary>
    /// Handles API error responses by parsing the error content and throwing an appropriate exception.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="TranslaasApiException">Always thrown with details from the error response.</exception>
    private async Task HandleApiError(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        // Read response content
#if NETSTANDARD2_0
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif

        // Attempt to parse error response as JSON
        TranslaasError? errorDetails = null;
        if (!string.IsNullOrWhiteSpace(responseContent))
        {
            try
            {
                errorDetails = JsonSerializer.Deserialize<TranslaasError>(responseContent, _jsonOptions);
            }
            catch (JsonException)
            {
                // If deserialization fails, we'll use the raw content
                errorDetails = null;
            }
        }

        // Build error message
        var errorMessage = errorDetails?.Message ?? $"API request failed with status code {response.StatusCode}.";
        
        // Include error code in message if available
        if (!string.IsNullOrWhiteSpace(errorDetails?.Code))
        {
            errorMessage = $"[{errorDetails.Code}] {errorMessage}";
        }

        // Create and throw exception
        throw new TranslaasApiException(
            errorMessage,
            response.StatusCode,
            innerException: null,
            responseContent: responseContent);
    }

    /// <summary>
    /// Builds a complete endpoint URL by combining the base URL with the endpoint path.
    /// </summary>
    /// <param name="endpoint">The endpoint path (e.g., "translations/text").</param>
    /// <returns>The complete URL.</returns>
    /// <exception cref="ArgumentNullException">Thrown when endpoint is null.</exception>
    internal string BuildEndpointUrl(string endpoint)
    {
        if (endpoint == null)
        {
            throw new ArgumentNullException(nameof(endpoint));
        }

        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var endpointPath = endpoint.TrimStart('/');

        return $"{baseUrl}/{endpointPath}";
    }

    /// <summary>
    /// Merges the number parameter into the parameters dictionary as "N".
    /// If "N" already exists in parameters, it takes precedence.
    /// </summary>
    /// <param name="number">The number parameter (for pluralization).</param>
    /// <param name="parameters">The existing parameters dictionary (may be null).</param>
    /// <returns>A new dictionary with number merged in, or null if both number and parameters are null/empty.</returns>
    private Dictionary<string, string>? MergeNumberIntoParameters(decimal? number, Dictionary<string, string>? parameters)
    {
        // If no number and no parameters, return null
        if (!number.HasValue && (parameters == null || parameters.Count == 0))
        {
            return null;
        }

        // Create a new dictionary to avoid modifying the input
        var merged = parameters == null 
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);

        // Only add N if it doesn't already exist (case-insensitive check)
        if (number.HasValue && !merged.ContainsKey("N"))
        {
            // Format number using invariant culture to ensure consistent formatting
            merged["N"] = number.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return merged.Count > 0 ? merged : null;
    }

    /// <summary>
    /// Builds an HTTP GET request message with JSON body and API key header.
    /// </summary>
    /// <typeparam name="T">The type of the request model.</typeparam>
    /// <param name="endpoint">The endpoint path.</param>
    /// <param name="requestModel">The request model to serialize as JSON.</param>
    /// <returns>An HttpRequestMessage configured for the API.</returns>
    /// <exception cref="ArgumentNullException">Thrown when endpoint or requestModel is null.</exception>
    internal HttpRequestMessage BuildGetRequest<T>(string endpoint, T requestModel) where T : class
    {
        return BuildGetRequest(endpoint, requestModel, null);
    }

    /// <summary>
    /// Builds an HTTP GET request message with query string parameters (from request model and additional parameters), API key header.
    /// The API expects GET requests with query string parameters, not JSON body.
    /// </summary>
    /// <typeparam name="T">The type of the request model.</typeparam>
    /// <param name="endpoint">The endpoint path.</param>
    /// <param name="requestModel">The request model to convert to query string parameters.</param>
    /// <param name="parameters">Optional dictionary of named parameters to append as query string parameters.</param>
    /// <returns>An HttpRequestMessage configured for the API.</returns>
    /// <exception cref="ArgumentNullException">Thrown when endpoint or requestModel is null.</exception>
    internal HttpRequestMessage BuildGetRequest<T>(string endpoint, T requestModel, Dictionary<string, string>? parameters) where T : class
    {
        if (endpoint == null)
        {
            throw new ArgumentNullException(nameof(endpoint));
        }

        if (requestModel == null)
        {
            throw new ArgumentNullException(nameof(requestModel));
        }

        var url = BuildEndpointUrl(endpoint);
        
        // Convert request model properties to query string parameters
        var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        // Use reflection to get all properties from the request model
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            var value = prop.GetValue(requestModel);
            if (value != null)
            {
                // Get JsonPropertyName attribute if present, otherwise use property name
                var jsonPropertyNameAttr = prop.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                    .FirstOrDefault() as JsonPropertyNameAttribute;
                var paramName = jsonPropertyNameAttr?.Name ?? prop.Name.ToLowerInvariant();
                
                // Convert value to string
                string stringValue = value switch
                {
                    string str => str,
                    decimal dec => dec.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    int i => i.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    float f => f.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    bool b => b.ToString().ToLowerInvariant(),
                    _ => value.ToString() ?? string.Empty
                };
                
                if (!string.IsNullOrEmpty(stringValue))
                {
                    queryParams[paramName] = stringValue;
                }
            }
        }
        
        // Merge additional parameters (these take precedence if there are conflicts)
        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    queryParams[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // Build query string
        if (queryParams.Count > 0)
        {
            var queryString = BuildQueryString(queryParams);
            if (!string.IsNullOrEmpty(queryString))
            {
                url += "?" + queryString;
            }
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        // Set API key header
        request.Headers.Add("X-Api-Key", _options.ApiKey);

        // Note: GET requests should not have a body - the API expects query string parameters only
        // Removed JSON body serialization

        return request;
    }

    /// <summary>
    /// Builds a query string from a dictionary of parameters, URL-encoding both keys and values.
    /// </summary>
    /// <param name="parameters">The parameters dictionary.</param>
    /// <returns>A URL-encoded query string (without the leading "?").</returns>
    private string BuildQueryString(Dictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return string.Empty;
        }

        var queryParts = new List<string>(parameters.Count);
        foreach (var kvp in parameters.Where(kvp => kvp.Key != null && kvp.Value != null))
        {
            // URL encode both key and value
            var encodedKey = Uri.EscapeDataString(kvp.Key);
            var encodedValue = Uri.EscapeDataString(kvp.Value);
            queryParts.Add($"{encodedKey}={encodedValue}");
        }

        return string.Join("&", queryParts);
    }
}
