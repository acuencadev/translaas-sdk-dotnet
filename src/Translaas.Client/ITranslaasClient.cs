using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Translaas.Models;
using Translaas.Models.Requests;
using Translaas.Models.Responses;

namespace Translaas.Client;

/// <summary>
/// Client interface for interacting with the Translaas Translation Delivery API.
/// </summary>
public interface ITranslaasClient
{
    /// <summary>
    /// Gets a single translation entry.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code (e.g., "en", "fr").</param>
    /// <param name="number">Optional number for pluralization.</param>
    /// <param name="parameters">Optional interpolation query parameters.</param>
    /// <param name="requestContext">Optional channel, version, project (when key is not project-scoped), and conditional GET options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text, or empty when <see cref="TranslaasRequestContext.NotModified"/> is <see langword="true"/>.</returns>
    /// <exception cref="Translaas.Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<string> GetEntryAsync(
        string group,
        string entry,
        string lang,
        decimal? number = null,
        Dictionary<string, string>? parameters = null,
        TranslaasRequestContext? requestContext = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all translations for a translation group.
    /// </summary>
    Task<TranslationGroup> GetGroupAsync(
        string project,
        string group,
        string lang,
        string? format = null,
        TranslaasRequestContext? requestContext = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all translations for a project.
    /// </summary>
    Task<TranslationProject> GetProjectAsync(
        string project,
        string lang,
        string? format = null,
        TranslaasRequestContext? requestContext = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available locales for a project.
    /// </summary>
    Task<ProjectLocales> GetProjectLocalesAsync(
        string project,
        TranslaasRequestContext? requestContext = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the offline translation ZIP bundle for a project.
    /// </summary>
    /// <param name="project">Project identifier.</param>
    /// <param name="requestContext">Optional channel, version, <c>includeContext</c>, and conditional GET (<c>If-None-Match</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>ZIP bytes or a not-modified result.</returns>
    Task<OfflineCacheDownloadResult> GetOfflineCacheAsync(
        string project,
        TranslaasRequestContext? requestContext = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports one or more missing translation keys (expects HTTP 202).
    /// </summary>
    /// <exception cref="Translaas.Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task ReportMissingKeysAsync(
        IEnumerable<ReportMissingKeyItemRequest> keys,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the configured API key (<c>GET /api/v1/api-keys/validate</c>).
    /// </summary>
    Task<ValidateApiKeyResponse> ValidateApiKeyAsync(CancellationToken cancellationToken = default);
}
