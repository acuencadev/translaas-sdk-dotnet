using System;
using System.Collections.Generic;

namespace Translaas.Caching.File.Models;

/// <summary>
/// Event arguments for cache synchronization completion events.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CacheSyncEventArgs"/> class.
/// </remarks>
/// <param name="project">The project that was synchronized.</param>
/// <param name="language">The language that was synchronized, or null for all languages.</param>
/// <param name="syncedAt">The timestamp of the synchronization.</param>
public class CacheSyncEventArgs(string project, string? language, DateTimeOffset syncedAt) : EventArgs
{

    /// <summary>
    /// Gets the project that was synchronized.
    /// </summary>
    public string Project { get; } = project ?? throw new ArgumentNullException(nameof(project));

    /// <summary>
    /// Gets the language that was synchronized, or null if all languages were synced.
    /// </summary>
    public string? Language { get; } = language;

    /// <summary>
    /// Gets the timestamp of the synchronization.
    /// </summary>
    public DateTimeOffset SyncedAt { get; } = syncedAt;
}

/// <summary>
/// Event arguments for cache synchronization failure events.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CacheSyncErrorEventArgs"/> class.
/// </remarks>
/// <param name="project">The project that failed to synchronize.</param>
/// <param name="language">The language that failed, or null for project-level failure.</param>
/// <param name="exception">The exception that caused the failure.</param>
public class CacheSyncErrorEventArgs(string project, string? language, Exception exception) : EventArgs
{

    /// <summary>
    /// Gets the project that failed to synchronize.
    /// </summary>
    public string Project { get; } = project ?? throw new ArgumentNullException(nameof(project));

    /// <summary>
    /// Gets the language that failed, or null for project-level failure.
    /// </summary>
    public string? Language { get; } = language;

    /// <summary>
    /// Gets the exception that caused the failure.
    /// </summary>
    public Exception Exception { get; } = exception ?? throw new ArgumentNullException(nameof(exception));
}

/// <summary>
/// Event arguments for complete synchronization events (all projects).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CacheSyncAllEventArgs"/> class.
/// </remarks>
/// <param name="syncedProjects">The list of projects that were successfully synchronized.</param>
/// <param name="failedProjects">The list of projects that failed to synchronize.</param>
/// <param name="syncedAt">The timestamp when synchronization completed.</param>
public class CacheSyncAllEventArgs(
    IReadOnlyList<string> syncedProjects,
    IReadOnlyList<string> failedProjects,
    DateTimeOffset syncedAt) : EventArgs
{

    /// <summary>
    /// Gets the list of projects that were successfully synchronized.
    /// </summary>
    public IReadOnlyList<string> SyncedProjects { get; } = syncedProjects ?? throw new ArgumentNullException(nameof(syncedProjects));

    /// <summary>
    /// Gets the list of projects that failed to synchronize.
    /// </summary>
    public IReadOnlyList<string> FailedProjects { get; } = failedProjects ?? throw new ArgumentNullException(nameof(failedProjects));

    /// <summary>
    /// Gets the timestamp when synchronization completed.
    /// </summary>
    public DateTimeOffset SyncedAt { get; } = syncedAt;
}
