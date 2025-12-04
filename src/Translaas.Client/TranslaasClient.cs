using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="options">The client options.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient or options is null.</exception>
    /// <exception cref="TranslaasConfigurationException">Thrown when options validation fails.</exception>
    public TranslaasClient(HttpClient httpClient, TranslaasClientOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        
        _options.Validate();
        
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
        int? number = null,
        CancellationToken cancellationToken = default)
    {
        // Parameter validation
        if (group == null)
        {
            throw new ArgumentNullException(nameof(group));
        }

        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (lang == null)
        {
            throw new ArgumentNullException(nameof(lang));
        }

        // Build request model
        var requestModel = new GetTranslationRequest
        {
            Group = group,
            Entry = entry,
            Lang = lang,
            Number = number
        };

        // Create HTTP request
        var request = BuildGetRequest("/api/translations/text", requestModel);

        try
        {
            // Send request
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            // Parse raw text response
#if NETSTANDARD2_0
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
            var result = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
            return result;
        }
        catch (TranslaasApiException)
        {
            // Re-throw API exceptions as-is
            throw;
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
        if (project == null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        if (group == null)
        {
            throw new ArgumentNullException(nameof(group));
        }

        if (lang == null)
        {
            throw new ArgumentNullException(nameof(lang));
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

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            // Deserialize JSON response
#if NETSTANDARD2_0
            var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
            var result = JsonSerializer.Deserialize<TranslationGroup>(jsonContent, _jsonOptions);
            
            if (result == null)
            {
                throw new TranslaasApiException(
                    "Failed to deserialize response from API.",
                    response.StatusCode,
                    responseContent: jsonContent);
            }

            return result;
        }
        catch (TranslaasApiException)
        {
            // Re-throw API exceptions as-is
            throw;
        }
        catch (JsonException ex)
        {
            throw new TranslaasApiException(
                $"Failed to deserialize response: {ex.Message}",
                HttpStatusCode.BadRequest,
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
        if (project == null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        if (lang == null)
        {
            throw new ArgumentNullException(nameof(lang));
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

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            // Deserialize JSON response
#if NETSTANDARD2_0
            var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
            var result = JsonSerializer.Deserialize<TranslationProject>(jsonContent, _jsonOptions);
            
            if (result == null)
            {
                throw new TranslaasApiException(
                    "Failed to deserialize response from API.",
                    response.StatusCode,
                    responseContent: jsonContent);
            }

            return result;
        }
        catch (TranslaasApiException)
        {
            // Re-throw API exceptions as-is
            throw;
        }
        catch (JsonException ex)
        {
            throw new TranslaasApiException(
                $"Failed to deserialize response: {ex.Message}",
                HttpStatusCode.BadRequest,
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
        if (project == null)
        {
            throw new ArgumentNullException(nameof(project));
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

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            // Deserialize JSON response
#if NETSTANDARD2_0
            var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
            var result = JsonSerializer.Deserialize<ProjectLocales>(jsonContent, _jsonOptions);
            
            if (result == null)
            {
                throw new TranslaasApiException(
                    "Failed to deserialize response from API.",
                    response.StatusCode,
                    responseContent: jsonContent);
            }

            return result;
        }
        catch (TranslaasApiException)
        {
            // Re-throw API exceptions as-is
            throw;
        }
        catch (JsonException ex)
        {
            throw new TranslaasApiException(
                $"Failed to deserialize response: {ex.Message}",
                HttpStatusCode.BadRequest,
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
    /// Builds an HTTP GET request message with JSON body and API key header.
    /// </summary>
    /// <typeparam name="T">The type of the request model.</typeparam>
    /// <param name="endpoint">The endpoint path.</param>
    /// <param name="requestModel">The request model to serialize as JSON.</param>
    /// <returns>An HttpRequestMessage configured for the API.</returns>
    /// <exception cref="ArgumentNullException">Thrown when endpoint or requestModel is null.</exception>
    internal HttpRequestMessage BuildGetRequest<T>(string endpoint, T requestModel) where T : class
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
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Set API key header
        request.Headers.Add("X-Api-Key", _options.ApiKey);

        // Serialize request model to JSON
        var json = JsonSerializer.Serialize(requestModel, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        request.Content = content;

        return request;
    }
}
