using System;

namespace Translaas.Models.Errors;

/// <summary>
/// Exception thrown when a translation is not found in the offline cache
/// and the API is unavailable.
/// </summary>
public class TranslaasOfflineCacheMissException : TranslaasOfflineCacheException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasOfflineCacheMissException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TranslaasOfflineCacheMissException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasOfflineCacheMissException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TranslaasOfflineCacheMissException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasOfflineCacheMissException"/> class.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="language">The language code.</param>
    /// <param name="group">The group name, if applicable.</param>
    /// <param name="entry">The entry key, if applicable.</param>
    public TranslaasOfflineCacheMissException(
        string project,
        string language,
        string? group = null,
        string? entry = null)
        : base(BuildMessage(project, language, group, entry), null, project, language, null)
    {
        Group = group;
        Entry = entry;
    }

    /// <summary>
    /// Gets the group name related to the cache miss, if applicable.
    /// </summary>
    public string? Group { get; }

    /// <summary>
    /// Gets the entry key related to the cache miss, if applicable.
    /// </summary>
    public string? Entry { get; }

    private static string BuildMessage(string project, string language, string? group, string? entry)
    {
        if (!string.IsNullOrEmpty(entry) && !string.IsNullOrEmpty(group))
        {
            return $"Translation entry '{entry}' in group '{group}' for project '{project}' and language '{language}' was not found in the offline cache.";
        }

        if (!string.IsNullOrEmpty(group))
        {
            return $"Translation group '{group}' for project '{project}' and language '{language}' was not found in the offline cache.";
        }

        return $"Project '{project}' for language '{language}' was not found in the offline cache.";
    }
}
