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
using Translaas.Models;
using Translaas.Models.Errors;
using Translaas.Models.Requests;
using Translaas.Models.Responses;

namespace Translaas.Client;

/// <summary>
/// Client implementation for interacting with the Translaas Translation Delivery API.
/// </summary>
public class TranslaasClient : ITranslaasClient
{
    private const string SdkTranslationsPrefix = "sdk/v1/translations";
    private const string ApiValidateKeyPath = "api/v1/api-keys/validate";

    private readonly HttpClient _httpClient;
    private readonly TranslaasClientOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly JsonSerializerOptions _jsonPostOptions;
    private readonly ITranslaasCacheProvider? _cacheProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasClient"/> class.
    /// </summary>
    public TranslaasClient(HttpClient httpClient, TranslaasClientOptions options, ITranslaasCacheProvider? cacheProvider = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _cacheProvider = cacheProvider;

        var skipApiValidation = string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.BaseUrl);
        _options.Validate(skipApiValidation);

        if (_options.Timeout.HasValue)
        {
            _httpClient.Timeout = _options.Timeout.Value;
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _jsonPostOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Creates a client after resolving <see cref="TranslaasClientOptions.DefaultProjectId"/> via validate when it is not configured.
    /// </summary>
    public static async Task<TranslaasClient> CreateAsync(
        HttpClient httpClient,
        TranslaasClientOptions options,
        ITranslaasCacheProvider? cacheProvider = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(options.DefaultProjectId))
        {
            return new TranslaasClient(httpClient, options, cacheProvider);
        }

        var temp = new TranslaasClient(httpClient, options, cacheProvider);
        var validate = await temp.ValidateApiKeyAsync(cancellationToken).ConfigureAwait(false);
        options.DefaultProjectId = ApiKeyProjectResolver.ResolveDefaultProjectId(options.DefaultProjectId, validate);
        return new TranslaasClient(httpClient, options, cacheProvider);
    }

    /// <inheritdoc />
    public async Task<string> GetEntryAsync(
        string group,
        string entry,
        string lang,
        decimal? number = null,
        Dictionary<string, string>? parameters = null,
        TranslaasRequestContext? requestContext = null,
        CancellationToken cancellationToken = default)
    {
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

        PrepareRequestContext(requestContext);

        var mergedParameters = MergeNumberIntoParameters(number, parameters);

        var ctxChannel = requestContext?.Channel;
        var ctxVersion = requestContext?.Version;
        var ctxProject = requestContext?.Project;

        if (_cacheProvider != null && _options.CacheMode == CacheMode.Entry)
        {
            var cacheKey = CacheKeyBuilder.BuildEntryKey(group, entry, lang, number, mergedParameters, ctxProject, ctxChannel, ctxVersion);
            if (_cacheProvider.TryGetValue<string>(cacheKey, out var cachedValue) && cachedValue != null)
            {
                return cachedValue;
            }
        }

        var requestModel = new GetTranslationRequest
        {
            Group = group,
            Entry = entry,
            Lang = lang,
            Number = number
        };
        ApplyContext(requestModel, requestContext, _options.DefaultProjectId);

        var request = BuildGetRequest($"{SdkTranslationsPrefix}/text", requestModel, mergedParameters, requestContext);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                AssignResponseContext(response, requestContext);
                return entry;
            }

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                AssignResponseContext(response, requestContext, notModified: true);
                return string.Empty;
            }

            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            AssignResponseContext(response, requestContext);

            var result = await ParseTextResponse(response, cancellationToken).ConfigureAwait(false);

            if (_cacheProvider != null && _options.CacheMode == CacheMode.Entry)
            {
                var cacheKey = CacheKeyBuilder.BuildEntryKey(group, entry, lang, number, mergedParameters, ctxProject, ctxChannel, ctxVersion);
                _cacheProvider.Set(cacheKey, result, _options.CacheAbsoluteExpiration, _options.CacheSlidingExpiration);
            }

            return result;
        }
        catch (TranslaasApiException)
        {
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
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
    public async Task<TranslationGroup> GetGroupAsync(
        string project,
        string group,
        string lang,
        string? format = null,
        TranslaasRequestContext? requestContext = null,
        CancellationToken cancellationToken = default)
    {
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

        PrepareRequestContext(requestContext);

        var ctxChannel = requestContext?.Channel;
        var ctxVersion = requestContext?.Version;
        var ctxInclude = requestContext?.IncludeContext;

        if (_cacheProvider != null && (_options.CacheMode == CacheMode.Group || _options.CacheMode == CacheMode.Project))
        {
            var cacheKey = CacheKeyBuilder.BuildGroupKey(project, group, lang, format, ctxChannel, ctxVersion, ctxInclude);
            if (_cacheProvider.TryGetValue<TranslationGroup>(cacheKey, out var cachedValue) && cachedValue != null)
            {
                return cachedValue;
            }
        }

        var requestModel = new GetGroupTranslationsRequest
        {
            Project = project,
            Group = group,
            Lang = lang,
            Format = format
        };
        ApplyContext(requestModel, requestContext);

        var request = BuildGetRequest($"{SdkTranslationsPrefix}/group", requestModel, null, requestContext);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                AssignResponseContext(response, requestContext);
                return new TranslationGroup();
            }

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                AssignResponseContext(response, requestContext, notModified: true);
                return new TranslationGroup();
            }

            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            AssignResponseContext(response, requestContext);

            var result = await ParseJsonResponse<TranslationGroup>(response, cancellationToken).ConfigureAwait(false);

            if (_cacheProvider != null && (_options.CacheMode == CacheMode.Group || _options.CacheMode == CacheMode.Project))
            {
                var cacheKey = CacheKeyBuilder.BuildGroupKey(project, group, lang, format, ctxChannel, ctxVersion, ctxInclude);
                _cacheProvider.Set(cacheKey, result, _options.CacheAbsoluteExpiration, _options.CacheSlidingExpiration);
            }

            return result;
        }
        catch (TranslaasApiException)
        {
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
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
        TranslaasRequestContext? requestContext = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(project))
        {
            throw new ArgumentNullException(nameof(project));
        }

        if (string.IsNullOrEmpty(lang))
        {
            throw new ArgumentNullException(nameof(lang));
        }

        PrepareRequestContext(requestContext);

        var ctxChannel = requestContext?.Channel;
        var ctxVersion = requestContext?.Version;
        var ctxInclude = requestContext?.IncludeContext;

        if (_cacheProvider != null && _options.CacheMode == CacheMode.Project)
        {
            var cacheKey = CacheKeyBuilder.BuildProjectKey(project, lang, format, ctxChannel, ctxVersion, ctxInclude);
            if (_cacheProvider.TryGetValue<TranslationProject>(cacheKey, out var cachedValue) && cachedValue != null)
            {
                return cachedValue;
            }
        }

        var requestModel = new GetProjectTranslationsRequest
        {
            Project = project,
            Lang = lang,
            Format = format
        };
        ApplyContext(requestModel, requestContext);

        var request = BuildGetRequest($"{SdkTranslationsPrefix}/project", requestModel, null, requestContext);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                AssignResponseContext(response, requestContext);
                return new TranslationProject();
            }

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                AssignResponseContext(response, requestContext, notModified: true);
                return new TranslationProject();
            }

            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            AssignResponseContext(response, requestContext);

            var result = await ParseJsonResponse<TranslationProject>(response, cancellationToken).ConfigureAwait(false);

            if (_cacheProvider != null && _options.CacheMode == CacheMode.Project)
            {
                var cacheKey = CacheKeyBuilder.BuildProjectKey(project, lang, format, ctxChannel, ctxVersion, ctxInclude);
                _cacheProvider.Set(cacheKey, result, _options.CacheAbsoluteExpiration, _options.CacheSlidingExpiration);
            }

            return result;
        }
        catch (TranslaasApiException)
        {
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
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
        TranslaasRequestContext? requestContext = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(project))
        {
            throw new ArgumentNullException(nameof(project));
        }

        PrepareRequestContext(requestContext);

        var ctxChannel = requestContext?.Channel;
        var ctxVersion = requestContext?.Version;

        if (_cacheProvider != null && _options.CacheMode != CacheMode.None)
        {
            var cacheKey = CacheKeyBuilder.BuildLocalesKey(project, ctxChannel, ctxVersion);
            if (_cacheProvider.TryGetValue<ProjectLocales>(cacheKey, out var cachedValue) && cachedValue != null)
            {
                return cachedValue;
            }
        }

        var requestModel = new GetProjectLocalesRequest { Project = project };
        ApplyContext(requestModel, requestContext);

        var request = BuildGetRequest($"{SdkTranslationsPrefix}/locales", requestModel, null, requestContext);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                AssignResponseContext(response, requestContext);
                return new ProjectLocales { Locales = [] };
            }

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                AssignResponseContext(response, requestContext, notModified: true);
                return new ProjectLocales { Locales = [], Project = project };
            }

            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            AssignResponseContext(response, requestContext);

            var result = await ParseJsonResponse<ProjectLocales>(response, cancellationToken).ConfigureAwait(false);

            if (_cacheProvider != null && _options.CacheMode != CacheMode.None)
            {
                var cacheKey = CacheKeyBuilder.BuildLocalesKey(project, ctxChannel, ctxVersion);
                _cacheProvider.Set(cacheKey, result, _options.CacheAbsoluteExpiration, _options.CacheSlidingExpiration);
            }

            return result;
        }
        catch (TranslaasApiException)
        {
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
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

    /// <inheritdoc />
    public async Task<OfflineCacheDownloadResult> GetOfflineCacheAsync(
        string project,
        TranslaasRequestContext? requestContext = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(project))
        {
            throw new ArgumentNullException(nameof(project));
        }

        PrepareRequestContext(requestContext);

        var requestModel = new GetOfflineCacheRequest { Project = project };
        ApplyContext(requestModel, requestContext);

        var request = BuildGetRequest($"{SdkTranslationsPrefix}/offline-cache", requestModel, null, requestContext);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            var etag304 = response.Headers.ETag?.ToString();
            AssignResponseContext(response, requestContext, notModified: true);
            return new OfflineCacheDownloadResult
            {
                NotModified = true,
                ETag = etag304,
                SuggestedFileName = null,
                Content = null
            };
        }

        if (!response.IsSuccessStatusCode)
        {
            await HandleApiError(response, cancellationToken).ConfigureAwait(false);
        }

        AssignResponseContext(response, requestContext);

        var responseEtag = response.Headers.ETag?.ToString();
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar?.Trim('"')
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"');

        byte[] content;
#if NETSTANDARD2_0
        content = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#else
        content = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#endif

        return new OfflineCacheDownloadResult
        {
            NotModified = false,
            ETag = responseEtag,
            SuggestedFileName = fileName,
            Content = content
        };
    }

    /// <inheritdoc />
    public async Task ReportMissingKeysAsync(IEnumerable<ReportMissingKeyItemRequest> keys, CancellationToken cancellationToken = default)
    {
        if (keys == null || !keys.Any())
        {
            return;
        }

        var requestModel = new ReportMissingKeysRequest { Keys = keys.ToList() };
        var request = BuildPostRequest($"{SdkTranslationsPrefix}/report-missing", requestModel);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (TranslaasApiException)
        {
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TranslaasApiException(
                $"Request timed out after {_httpClient.Timeout.TotalSeconds} seconds.",
                HttpStatusCode.RequestTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new TranslaasApiException(
                $"Failed to report missing keys: {ex.Message}",
                HttpStatusCode.BadRequest,
                ex);
        }
    }

    /// <inheritdoc />
    public async Task<ValidateApiKeyResponse> ValidateApiKeyAsync(CancellationToken cancellationToken = default)
    {
        var url = BuildEndpointUrl(ApiValidateKeyPath);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Api-Key", _options.ApiKey);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                await HandleApiError(response, cancellationToken).ConfigureAwait(false);
            }

            return await ParseJsonResponse<ValidateApiKeyResponse>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (TranslaasApiException)
        {
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TranslaasApiException(
                $"Request timed out after {_httpClient.Timeout.TotalSeconds} seconds.",
                HttpStatusCode.RequestTimeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new TranslaasApiException(
                $"Failed to validate API key: {ex.Message}",
                HttpStatusCode.BadRequest,
                ex);
        }
    }

    private static void PrepareRequestContext(TranslaasRequestContext? ctx)
    {
        if (ctx == null)
        {
            return;
        }

        ctx.NotModified = false;
        ctx.ResponseEtag = null;
    }

    private static void AssignResponseContext(HttpResponseMessage response, TranslaasRequestContext? ctx, bool notModified = false)
    {
        if (ctx == null)
        {
            return;
        }

        ctx.NotModified = notModified;
        if (response.Headers.ETag != null)
        {
            ctx.ResponseEtag = response.Headers.ETag.ToString();
        }
    }

    private static void ApplyContext(GetTranslationRequest model, TranslaasRequestContext? ctx, string? defaultProjectId)
    {
        if (!string.IsNullOrEmpty(ctx?.Project))
        {
            model.Project = ctx.Project;
        }
        else if (!string.IsNullOrEmpty(defaultProjectId))
        {
            model.Project = defaultProjectId;
        }

        if (ctx == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(ctx.Channel))
        {
            model.Channel = ctx.Channel;
        }

        if (!string.IsNullOrEmpty(ctx.Version))
        {
            model.Version = ctx.Version;
        }
    }

    private static void ApplyContext(GetGroupTranslationsRequest model, TranslaasRequestContext? ctx)
    {
        if (ctx == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(ctx.Channel))
        {
            model.Channel = ctx.Channel;
        }

        if (!string.IsNullOrEmpty(ctx.Version))
        {
            model.Version = ctx.Version;
        }

        if (ctx.IncludeContext.HasValue)
        {
            model.IncludeContext = ctx.IncludeContext;
        }
    }

    private static void ApplyContext(GetProjectTranslationsRequest model, TranslaasRequestContext? ctx)
    {
        if (ctx == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(ctx.Channel))
        {
            model.Channel = ctx.Channel;
        }

        if (!string.IsNullOrEmpty(ctx.Version))
        {
            model.Version = ctx.Version;
        }

        if (ctx.IncludeContext.HasValue)
        {
            model.IncludeContext = ctx.IncludeContext;
        }
    }

    private static void ApplyContext(GetProjectLocalesRequest model, TranslaasRequestContext? ctx)
    {
        if (ctx == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(ctx.Channel))
        {
            model.Channel = ctx.Channel;
        }

        if (!string.IsNullOrEmpty(ctx.Version))
        {
            model.Version = ctx.Version;
        }
    }

    private static void ApplyContext(GetOfflineCacheRequest model, TranslaasRequestContext? ctx)
    {
        if (ctx == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(ctx.Channel))
        {
            model.Channel = ctx.Channel;
        }

        if (!string.IsNullOrEmpty(ctx.Version))
        {
            model.Version = ctx.Version;
        }

        if (ctx.IncludeContext.HasValue)
        {
            model.IncludeContext = ctx.IncludeContext;
        }
    }

    private HttpRequestMessage BuildPostRequest<T>(string endpoint, T requestModel) where T : class
    {
        var url = BuildEndpointUrl(endpoint);
        var json = JsonSerializer.Serialize(requestModel, _jsonPostOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        request.Headers.Add("X-Api-Key", _options.ApiKey);

        return request;
    }

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

        return result ?? string.Empty;
    }

    private async Task<T> ParseJsonResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken) where T : class
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

#if NETSTANDARD2_0
        var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif

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

    private async Task HandleApiError(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

#if NETSTANDARD2_0
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif

        TranslaasError? errorDetails = null;
        if (!string.IsNullOrWhiteSpace(responseContent))
        {
            try
            {
                errorDetails = JsonSerializer.Deserialize<TranslaasError>(responseContent, _jsonOptions);
            }
            catch (JsonException)
            {
                errorDetails = null;
            }
        }

        var errorMessage = errorDetails?.Message ?? $"API request failed with status code {response.StatusCode}.";

        if (!string.IsNullOrWhiteSpace(errorDetails?.Code))
        {
            errorMessage = $"[{errorDetails.Code}] {errorMessage}";
        }

        throw new TranslaasApiException(
            errorMessage,
            response.StatusCode,
            innerException: null,
            responseContent: responseContent);
    }

    /// <summary>
    /// Builds a complete endpoint URL by combining the base URL with the endpoint path.
    /// </summary>
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

    private Dictionary<string, string>? MergeNumberIntoParameters(decimal? number, Dictionary<string, string>? parameters)
    {
        if (!number.HasValue && (parameters == null || parameters.Count == 0))
        {
            return null;
        }

        var merged = parameters == null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);

        if (number.HasValue && !merged.ContainsKey("N"))
        {
            merged["N"] = number.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return merged.Count > 0 ? merged : null;
    }

    internal HttpRequestMessage BuildGetRequest<T>(string endpoint, T requestModel) where T : class
    {
        return BuildGetRequest(endpoint, requestModel, null, null);
    }

    internal HttpRequestMessage BuildGetRequest<T>(string endpoint, T requestModel, Dictionary<string, string>? parameters) where T : class
    {
        return BuildGetRequest(endpoint, requestModel, parameters, null);
    }

    internal HttpRequestMessage BuildGetRequest<T>(string endpoint, T requestModel, Dictionary<string, string>? parameters, TranslaasRequestContext? requestContext) where T : class
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

        var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            var value = prop.GetValue(requestModel);
            if (value != null)
            {
                var jsonPropertyNameAttr = prop.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                    .FirstOrDefault() as JsonPropertyNameAttribute;
                var paramName = jsonPropertyNameAttr?.Name ?? prop.Name.ToLowerInvariant();

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

        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    var existingKey = queryParams.Keys.FirstOrDefault(k =>
                        string.Equals(k, kvp.Key, StringComparison.OrdinalIgnoreCase));
                    if (existingKey != null)
                    {
                        queryParams.Remove(existingKey);
                    }

                    queryParams[kvp.Key] = kvp.Value;
                }
            }
        }

        if (queryParams.Count > 0)
        {
            var queryString = BuildQueryString(queryParams);
            if (!string.IsNullOrEmpty(queryString))
            {
                url += "?" + queryString;
            }
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("X-Api-Key", _options.ApiKey);

        if (!string.IsNullOrEmpty(requestContext?.IfNoneMatch))
        {
            request.Headers.TryAddWithoutValidation("If-None-Match", requestContext.IfNoneMatch);
        }

        return request;
    }

    private string BuildQueryString(Dictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return string.Empty;
        }

        var queryParts = new List<string>(parameters.Count);
        foreach (var kvp in parameters.Where(kvp => kvp.Key != null && kvp.Value != null))
        {
            var encodedKey = Uri.EscapeDataString(kvp.Key);
            var encodedValue = Uri.EscapeDataString(kvp.Value);
            queryParts.Add($"{encodedKey}={encodedValue}");
        }

        return string.Join("&", queryParts);
    }
}
