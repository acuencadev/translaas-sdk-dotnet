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
    /// <param name="number">Optional number for pluralization. Supports both integer and decimal/fractional numbers (e.g., 1.31). When provided, automatically populates the "N" parameter for placeholder replacement.</param>
    /// <param name="parameters">Optional dictionary of named parameters to inject into translation placeholders (e.g., {{"userName", "John"}, {"pending", "3"}}). Parameter names are case-insensitive.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="Translaas.Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    /// <remarks>
    /// <para>
    /// Named parameters are passed to the API endpoint as query string parameters and are used to replace placeholders in translation strings.
    /// For example, if a translation contains "Hello {userName}, you have {N} items", pass parameters: {{"userName", "John"}, {"N", "5"}}.
    /// </para>
    /// <para>
    /// If both <paramref name="number"/> and a parameter named "N" are provided, the "N" parameter takes precedence.
    /// </para>
    /// </remarks>
    Task<string> GetEntryAsync(
        string group,
        string entry,
        string lang,
        decimal? number = null,
        System.Collections.Generic.Dictionary<string, string>? parameters = null,
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
