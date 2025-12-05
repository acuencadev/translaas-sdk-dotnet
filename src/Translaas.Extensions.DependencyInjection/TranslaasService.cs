using System.Threading;
using System.Threading.Tasks;

using Translaas.Client;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Convenience service implementation for translation lookups with a simplified API.
/// </summary>
/// <remarks>
/// This service wraps <see cref="ITranslaasClient"/> to provide a more convenient API
/// for common translation lookups.
/// </remarks>
public class TranslaasService : ITranslaasService
{
    private readonly ITranslaasClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasService"/> class.
    /// </summary>
    /// <param name="client">The Translaas client to use for translation lookups.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when client is null.</exception>
    public TranslaasService(ITranslaasClient client)
    {
        _client = client ?? throw new System.ArgumentNullException(nameof(client));
    }

    /// <inheritdoc />
    public Task<string> T(
        string group,
        string entry,
        string lang,
        int? number = null,
        CancellationToken cancellationToken = default)
    {
        return _client.GetEntryAsync(group, entry, lang, number, cancellationToken);
    }
}
