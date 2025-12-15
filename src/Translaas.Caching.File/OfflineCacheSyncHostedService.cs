using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Translaas.Caching.File.Models;

namespace Translaas.Caching.File;

/// <summary>
/// A hosted service that performs background synchronization of the offline cache.
/// Implements <see cref="IHostedService"/> for integration with ASP.NET Core's hosted services.
/// </summary>
/// <remarks>
/// <para>
/// This service starts background synchronization when the application starts and
/// stops it gracefully when the application shuts down.
/// </para>
/// <para>
/// The synchronization behavior is controlled by <see cref="OfflineCacheOptions"/>:
/// </para>
/// <list type="bullet">
/// <item><description>If <see cref="OfflineCacheOptions.AutoSync"/> is false, no synchronization occurs.</description></item>
/// <item><description>On startup, all configured projects/languages are synchronized.</description></item>
/// <item><description>If <see cref="OfflineCacheOptions.AutoSyncInterval"/> is set, periodic sync runs at that interval.</description></item>
/// </list>
/// </remarks>
public class OfflineCacheSyncHostedService : IHostedService, IDisposable
{
    private readonly IOfflineCacheSyncService _syncService;
    private readonly OfflineCacheOptions _options;
    private readonly ILogger<OfflineCacheSyncHostedService>? _logger;

    private CancellationTokenSource? _stoppingCts;
    private Task? _executingTask;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfflineCacheSyncHostedService"/> class.
    /// </summary>
    /// <param name="syncService">The offline cache sync service.</param>
    /// <param name="options">The offline cache options.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when syncService or options is null.</exception>
    public OfflineCacheSyncHostedService(
        IOfflineCacheSyncService syncService,
        OfflineCacheOptions options,
        ILogger<OfflineCacheSyncHostedService>? logger = null)
    {
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        // Subscribe to sync events for logging
        _syncService.SyncCompleted += OnSyncCompleted;
        _syncService.SyncFailed += OnSyncFailed;
        _syncService.SyncAllCompleted += OnSyncAllCompleted;
    }

    /// <summary>
    /// Starts the background synchronization service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the startup operation.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled || !_options.AutoSync)
        {
            _logger?.LogInformation("Offline cache sync is disabled. Skipping background sync.");
            return Task.CompletedTask;
        }

        if (_options.Projects.Count == 0)
        {
            _logger?.LogWarning("Offline cache sync is enabled but no projects are configured. Skipping background sync.");
            return Task.CompletedTask;
        }

        _logger?.LogInformation(
            "Starting offline cache sync hosted service for {ProjectCount} project(s) with {LanguageCount} language(s).",
            _options.Projects.Count,
            _options.Languages.Count > 0 ? _options.Languages.Count : "all");

        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = ExecuteAsync(_stoppingCts.Token);

        // If the task completed synchronously, return it
        if (_executingTask.IsCompleted)
        {
            return _executingTask;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the background synchronization service gracefully.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the shutdown operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null)
        {
            return;
        }

        _logger?.LogInformation("Stopping offline cache sync hosted service...");

        try
        {
            // Signal cancellation
#if NETSTANDARD2_0
            _stoppingCts?.Cancel();
#else
            if (_stoppingCts != null)
            {
                await _stoppingCts.CancelAsync().ConfigureAwait(false);
            }
#endif
        }
        finally
        {
            // Wait for the background task to complete (with a timeout)
            var completedTask = await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);

            if (completedTask != _executingTask)
            {
                _logger?.LogWarning("Offline cache sync hosted service did not stop gracefully within the timeout.");
            }
        }

        _logger?.LogInformation("Offline cache sync hosted service stopped.");
    }

    /// <summary>
    /// Executes the background synchronization loop.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token that signals when the service should stop.</param>
    /// <returns>A task representing the background operation.</returns>
    protected virtual async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Perform initial sync
        await PerformSyncAsync(stoppingToken).ConfigureAwait(false);

        // If no interval is configured, we're done after initial sync
        if (!_options.AutoSyncInterval.HasValue)
        {
            _logger?.LogInformation("No auto-sync interval configured. Initial sync complete.");
            return;
        }

        _logger?.LogInformation(
            "Background sync will run every {Interval}.",
            _options.AutoSyncInterval.Value);

        // Periodic sync loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_options.AutoSyncInterval.Value, stoppingToken).ConfigureAwait(false);
                await PerformSyncAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Expected when stopping
                break;
            }
        }
    }

    private async Task PerformSyncAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger?.LogDebug("Starting offline cache synchronization...");
            await _syncService.SyncAllAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger?.LogDebug("Offline cache synchronization was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during offline cache synchronization. Will retry on next interval.");
            // Don't rethrow - we want to continue the sync loop
        }
    }

    private void OnSyncCompleted(object? sender, CacheSyncEventArgs e)
    {
        _logger?.LogDebug(
            "Synced project '{Project}' language '{Language}' at {SyncedAt}.",
            e.Project,
            e.Language ?? "all",
            e.SyncedAt);
    }

    private void OnSyncFailed(object? sender, CacheSyncErrorEventArgs e)
    {
        _logger?.LogWarning(
            e.Exception,
            "Failed to sync project '{Project}' language '{Language}'.",
            e.Project,
            e.Language ?? "all");
    }

    private void OnSyncAllCompleted(object? sender, CacheSyncAllEventArgs e)
    {
        if (e.FailedProjects.Count == 0)
        {
            _logger?.LogInformation(
                "Offline cache synchronization completed successfully. Synced {SyncedCount} project(s).",
                e.SyncedProjects.Count);
        }
        else
        {
            _logger?.LogWarning(
                "Offline cache synchronization completed with errors. Synced: {SyncedCount}, Failed: {FailedCount}. Failed projects: {FailedProjects}",
                e.SyncedProjects.Count,
                e.FailedProjects.Count,
                string.Join(", ", e.FailedProjects));
        }
    }

    /// <summary>
    /// Releases the resources used by the service.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _syncService.SyncCompleted -= OnSyncCompleted;
            _syncService.SyncFailed -= OnSyncFailed;
            _syncService.SyncAllCompleted -= OnSyncAllCompleted;

            _stoppingCts?.Cancel();
            _stoppingCts?.Dispose();
        }

        _disposed = true;
    }
}
