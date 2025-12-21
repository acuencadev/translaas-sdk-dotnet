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
/// <see cref="Client.ITranslaasClient.GetEntryAsync(string, string, string, decimal?, System.Threading.CancellationToken)"/>.
/// </para>
/// </remarks>
public interface ITranslaasService
{
    /// <summary>
    /// Gets a translation entry (shorthand for GetEntryAsync).
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code (e.g., "en", "fr").</param>
    /// <param name="number">Optional number for pluralization. Supports both integer and decimal/fractional numbers (e.g., 1.31).</param>
    /// <param name="parameters">Optional dictionary of named parameters to use in translation placeholders (e.g., {userName}, {count}).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="Models.Errors.TranslaasApiException">Thrown when the API returns an error.</exception>
    /// <example>
    /// <code>
    /// var translation = await _translaas.T("common", "welcome", "en");
    /// var plural = await _translaas.T("messages", "item", "en", 5);
    /// var fractional = await _translaas.T("messages", "item", "en", 1.31m);
    /// var withParams = await _translaas.T("messages", "greeting", "en", parameters: new Dictionary&lt;string, string&gt; { { "userName", "John" } });
    /// </code>
    /// </example>
    Task<string> T(
        string group,
        string entry,
        string lang,
        decimal? number = null,
        Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default);
}
