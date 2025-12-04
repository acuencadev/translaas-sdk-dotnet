using System.Threading;
using System.Threading.Tasks;

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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="Translaas.Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<string> GetEntryAsync(
        string group,
        string entry,
        string lang,
        int? number = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all translations for a translation group.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="group">The translation group name.</param>
    /// <param name="lang">The language code (e.g., "en", "fr").</param>
    /// <param name="format">Optional format parameter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="TranslationGroup"/> containing all entries for the group.</returns>
    /// <exception cref="Translaas.Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<TranslationGroup> GetGroupAsync(
        string project,
        string group,
        string lang,
        string? format = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all translations for a project.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="lang">The language code (e.g., "en", "fr").</param>
    /// <param name="format">Optional format parameter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="TranslationProject"/> containing all groups and entries.</returns>
    /// <exception cref="Translaas.Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<TranslationProject> GetProjectAsync(
        string project,
        string lang,
        string? format = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available locales for a project.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="ProjectLocales"/> object containing available locales.</returns>
    /// <exception cref="Translaas.Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<ProjectLocales> GetProjectLocalesAsync(
        string project,
        CancellationToken cancellationToken = default);
}
