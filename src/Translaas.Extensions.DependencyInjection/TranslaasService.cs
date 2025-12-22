using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Translaas.Client;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Convenience service implementation for translation lookups with a simplified API.
/// </summary>
/// <remarks>
/// This service wraps <see cref="ITranslaasClient"/> to provide a more convenient API
/// for common translation lookups. Supports automatic language resolution when providers are configured.
/// </remarks>
public class TranslaasService : ITranslaasService
{
    private readonly ITranslaasClient _client;
    private readonly ILanguageResolver? _resolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasService"/> class.
    /// </summary>
    /// <param name="client">The Translaas client to use for translation lookups.</param>
    /// <param name="resolver">Optional language resolver for automatic language resolution.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when client is null.</exception>
    public TranslaasService(ITranslaasClient client, ILanguageResolver? resolver = null)
    {
        _client = client ?? throw new System.ArgumentNullException(nameof(client));
        _resolver = resolver;
    }

    /// <inheritdoc />
    public Task<string> T(
        string group,
        string entry,
        string? lang = null,
        decimal? number = null,
        Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        // If lang is explicitly provided, use it (highest priority)
        if (!string.IsNullOrWhiteSpace(lang))
        {
            return _client.GetEntryAsync(group, entry, lang, number, parameters, cancellationToken);
        }

        // Otherwise, try to resolve from providers
        if (_resolver != null)
        {
            var resolvedLang = _resolver.Resolve();
            if (resolvedLang != null && !string.IsNullOrWhiteSpace(resolvedLang))
            {
                return _client.GetEntryAsync(group, entry, resolvedLang, number, parameters, cancellationToken);
            }
        }

        // No language resolved - throw exception
        throw new System.InvalidOperationException(
            "Unable to determine language for translation request. " +
            "Either provide the 'lang' parameter explicitly, or configure language providers " +
            "using AddTranslaas(..., language => language.UseCulture().UseDefault()).");
    }
}
