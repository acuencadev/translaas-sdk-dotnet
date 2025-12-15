using System;
using System.Collections.Generic;

namespace Translaas.Caching.File.Models;

/// <summary>
/// Event arguments for cache synchronization completion events.
/// </summary>
public class CacheSyncEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheSyncEventArgs"/> class.
    /// </summary>
    /// <param name="project">The project that was synchronized.</param>
    /// <param name="language">The language that was synchronized, or null for all languages.</param>
    /// <param name="syncedAt">The timestamp of the synchronization.</param>
    public CacheSyncEventArgs(string project, string? language, DateTimeOffset syncedAt)
    {
        Project = project ?? throw new ArgumentNullException(nameof(project));
        Language = language;
        SyncedAt = syncedAt;
    }

    /// <summary>
    /// Gets the project that was synchronized.
    /// </summary>
    public string Project { get; }

    /// <summary>
    /// Gets the language that was synchronized, or null if all languages were synced.
    /// </summary>
    public string? Language { get; }

    /// <summary>
    /// Gets the timestamp of the synchronization.
    /// </summary>
    public DateTimeOffset SyncedAt { get; }
}

/// <summary>
/// Event arguments for cache synchronization failure events.
/// </summary>
public class CacheSyncErrorEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheSyncErrorEventArgs"/> class.
    /// </summary>
    /// <param name="project">The project that failed to synchronize.</param>
    /// <param name="language">The language that failed, or null for project-level failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public CacheSyncErrorEventArgs(string project, string? language, Exception exception)
    {
        Project = project ?? throw new ArgumentNullException(nameof(project));
        Language = language;
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    /// <summary>
    /// Gets the project that failed to synchronize.
    /// </summary>
    public string Project { get; }

    /// <summary>
    /// Gets the language that failed, or null for project-level failure.
    /// </summary>
    public string? Language { get; }

    /// <summary>
    /// Gets the exception that caused the failure.
    /// </summary>
    public Exception Exception { get; }
}

/// <summary>
/// Event arguments for complete synchronization events (all projects).
/// </summary>
public class CacheSyncAllEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheSyncAllEventArgs"/> class.
    /// </summary>
    /// <param name="syncedProjects">The list of projects that were successfully synchronized.</param>
    /// <param name="failedProjects">The list of projects that failed to synchronize.</param>
    /// <param name="syncedAt">The timestamp when synchronization completed.</param>
    public CacheSyncAllEventArgs(
        IReadOnlyList<string> syncedProjects,
        IReadOnlyList<string> failedProjects,
        DateTimeOffset syncedAt)
    {
        SyncedProjects = syncedProjects ?? throw new ArgumentNullException(nameof(syncedProjects));
        FailedProjects = failedProjects ?? throw new ArgumentNullException(nameof(failedProjects));
        SyncedAt = syncedAt;
    }

    /// <summary>
    /// Gets the list of projects that were successfully synchronized.
    /// </summary>
    public IReadOnlyList<string> SyncedProjects { get; }

    /// <summary>
    /// Gets the list of projects that failed to synchronize.
    /// </summary>
    public IReadOnlyList<string> FailedProjects { get; }

    /// <summary>
    /// Gets the timestamp when synchronization completed.
    /// </summary>
    public DateTimeOffset SyncedAt { get; }
}
