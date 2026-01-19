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
    /// <example>
    /// <code>
    /// // Basic usage
    /// string translation = await client.GetEntryAsync("common", "welcome", "en");
    /// 
    /// // With pluralization
    /// string message = await client.GetEntryAsync("messages", "item", "en", number: 5);
    /// 
    /// // With named parameters
    /// var parameters = new Dictionary&lt;string, string&gt; { { "userName", "John" } };
    /// string greeting = await client.GetEntryAsync("messages", "greeting", "en", parameters: parameters);
    /// 
    /// // With pluralization and parameters
    /// var params = new Dictionary&lt;string, string&gt; { { "userName", "John" } };
    /// string items = await client.GetEntryAsync("messages", "items", "en", number: 5, parameters: params);
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// TranslationGroup group = await client.GetGroupAsync("my-project", "ui", "en");
    /// 
    /// // Access entries
    /// foreach (var entry in group.Entries)
    /// {
    ///     Console.WriteLine($"{entry.Key}: {entry.Value}");
    /// }
    /// 
    /// // Get specific value
    /// string welcome = group.GetValue("welcome");
    /// 
    /// // Check for plural forms
    /// if (group.HasPluralForms("item"))
    /// {
    ///     var pluralForms = group.GetPluralForms("item");
    /// }
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// TranslationProject project = await client.GetProjectAsync("my-project", "en");
    /// 
    /// // Access groups
    /// foreach (var groupEntry in project.Groups)
    /// {
    ///     var group = project.GetGroup(groupEntry.Key);
    ///     if (group != null)
    ///     {
    ///         Console.WriteLine($"Group: {groupEntry.Key}");
    ///         foreach (var entry in group.Entries)
    ///         {
    ///             Console.WriteLine($"  {entry.Key}: {entry.Value}");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// ProjectLocales locales = await client.GetProjectLocalesAsync("my-project");
    /// 
    /// foreach (string locale in locales.Locales)
    /// {
    ///     Console.WriteLine($"Available locale: {locale}");
    /// }
    /// </code>
    /// </example>
    Task<ProjectLocales> GetProjectLocalesAsync(
        string project,
        CancellationToken cancellationToken = default);
}
