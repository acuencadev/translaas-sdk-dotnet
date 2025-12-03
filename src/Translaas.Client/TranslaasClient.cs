using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Translaas.Models.Errors;

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
    public Task<string> GetEntryAsync(
        string group,
        string entry,
        string lang,
        int? number = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<Translaas.Models.Responses.TranslationGroup> GetGroupAsync(
        string project,
        string group,
        string lang,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<Translaas.Models.Responses.TranslationProject> GetProjectAsync(
        string project,
        string lang,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<Translaas.Models.Responses.ProjectLocales> GetProjectLocalesAsync(
        string project,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
