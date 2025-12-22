using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Convenience service interface for translation lookups with a simplified API.
/// </summary>
/// <remarks>
/// <para>
/// This service provides a convenient wrapper around <see cref="Client.ITranslaasClient"/> 
/// with a simplified method signature for common translation lookups.
/// </para>
/// <para>
/// The <c>T</c> method is a shorthand for 
/// <see cref="Client.ITranslaasClient.GetEntryAsync(string, string, string, decimal?, System.Collections.Generic.Dictionary{string, string}?, System.Threading.CancellationToken)"/>.
/// </para>
/// </remarks>
public interface ITranslaasService
{
    /// <summary>
    /// Gets a translation entry using automatic language resolution from configured providers.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when no provider returns a language.</exception>
    /// <exception cref="Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<string> T(string group, string entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a translation entry with explicit language override.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code (e.g., "en", "fr"). Bypasses all language providers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<string> T(string group, string entry, string lang, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a translation entry with explicit language and pluralization number.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code (e.g., "en", "fr"). Bypasses all language providers.</param>
    /// <param name="number">Number for pluralization. Supports both integer and decimal/fractional numbers (e.g., 1.31).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<string> T(string group, string entry, string lang, decimal number, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a translation entry with explicit language and named parameters.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code (e.g., "en", "fr"). Bypasses all language providers.</param>
    /// <param name="parameters">Dictionary of named parameters to use in translation placeholders (e.g., {userName}, {count}).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<string> T(string group, string entry, string lang, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a translation entry with explicit language, pluralization number, and named parameters.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code (e.g., "en", "fr"). Bypasses all language providers.</param>
    /// <param name="number">Number for pluralization. Supports both integer and decimal/fractional numbers (e.g., 1.31).</param>
    /// <param name="parameters">Dictionary of named parameters to use in translation placeholders (e.g., {userName}, {count}).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<string> T(string group, string entry, string lang, decimal number, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a translation entry using automatic language resolution with pluralization number.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="number">Number for pluralization. Supports both integer and decimal/fractional numbers (e.g., 1.31).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when no provider returns a language.</exception>
    /// <exception cref="Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<string> T(string group, string entry, decimal number, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a translation entry using automatic language resolution with named parameters.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="parameters">Dictionary of named parameters to use in translation placeholders (e.g., {userName}, {count}).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when no provider returns a language.</exception>
    /// <exception cref="Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<string> T(string group, string entry, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a translation entry using automatic language resolution with pluralization number and named parameters.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="number">Number for pluralization. Supports both integer and decimal/fractional numbers (e.g., 1.31).</param>
    /// <param name="parameters">Dictionary of named parameters to use in translation placeholders (e.g., {userName}, {count}).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when no provider returns a language.</exception>
    /// <exception cref="Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    Task<string> T(string group, string entry, decimal number, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
}
