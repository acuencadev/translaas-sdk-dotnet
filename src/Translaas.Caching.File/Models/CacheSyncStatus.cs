namespace Translaas.Caching.File.Models;

/// <summary>
/// Status of cache synchronization for a project.
/// </summary>
public enum CacheSyncStatus
{
    /// <summary>
    /// Project is queued for synchronization but not yet started.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Project is currently being synchronized.
    /// </summary>
    Syncing = 1,

    /// <summary>
    /// Project has been successfully synchronized.
    /// </summary>
    Synced = 2,

    /// <summary>
    /// Synchronization failed for this project.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Cache exists but is considered stale (older than sync interval).
    /// </summary>
    Stale = 4
}
