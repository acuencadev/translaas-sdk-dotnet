using System;
using System.Threading;
using System.Threading.Tasks;

using Translaas.Caching.File.Models;

namespace Translaas.Caching.File;

/// <summary>
/// Service for synchronizing offline cache with the Translaas API.
/// </summary>
public interface IOfflineCacheSyncService
{
    /// <summary>
    /// Synchronizes a specific project and language to the cache.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="lang">The language code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SyncProjectAsync(
        string project,
        string lang,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes all languages for a specific project to the cache.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SyncProjectAllLanguagesAsync(
        string project,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes all configured projects and languages.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SyncAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts background synchronization based on configured interval.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StartBackgroundSyncAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops background synchronization.
    /// </summary>
    Task StopBackgroundSyncAsync();

    /// <summary>
    /// Gets whether background synchronization is currently running.
    /// </summary>
    bool IsBackgroundSyncRunning { get; }

    /// <summary>
    /// Event raised when a project/language synchronization completes successfully.
    /// </summary>
    event EventHandler<CacheSyncEventArgs>? SyncCompleted;

    /// <summary>
    /// Event raised when a project/language synchronization fails.
    /// </summary>
    event EventHandler<CacheSyncErrorEventArgs>? SyncFailed;

    /// <summary>
    /// Event raised when full synchronization (all projects) completes.
    /// </summary>
    event EventHandler<CacheSyncAllEventArgs>? SyncAllCompleted;
}
