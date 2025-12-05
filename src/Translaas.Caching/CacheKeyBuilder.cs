using System;
using System.Text;

namespace Translaas.Caching;

/// <summary>
/// Helper class for building consistent cache keys for Translaas translation data.
/// </summary>
public static class CacheKeyBuilder
{
    private const string KeySeparator = ":";

    /// <summary>
    /// Builds a cache key for a single translation entry.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code.</param>
    /// <param name="number">Optional number for pluralization.</param>
    /// <returns>A cache key in the format: "entry:group:entry:lang[:number]".</returns>
    /// <exception cref="ArgumentNullException">Thrown when group, entry, or lang is null.</exception>
    public static string BuildEntryKey(string group, string entry, string lang, int? number = null)
    {
        if (group == null)
        {
            throw new ArgumentNullException(nameof(group));
        }

        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (lang == null)
        {
            throw new ArgumentNullException(nameof(lang));
        }

        var keyBuilder = new StringBuilder("entry");
        keyBuilder.Append(KeySeparator);
        keyBuilder.Append(group);
        keyBuilder.Append(KeySeparator);
        keyBuilder.Append(entry);
        keyBuilder.Append(KeySeparator);
        keyBuilder.Append(lang);

        if (number.HasValue)
        {
            keyBuilder.Append(KeySeparator);
            keyBuilder.Append(number.Value);
        }

        return keyBuilder.ToString();
    }

    /// <summary>
    /// Builds a cache key for a translation group.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="group">The translation group name.</param>
    /// <param name="lang">The language code.</param>
    /// <param name="format">Optional format parameter.</param>
    /// <returns>A cache key in the format: "group:project:group:lang[:format]".</returns>
    /// <exception cref="ArgumentNullException">Thrown when project, group, or lang is null.</exception>
    public static string BuildGroupKey(string project, string group, string lang, string? format = null)
    {
        if (project == null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        if (group == null)
        {
            throw new ArgumentNullException(nameof(group));
        }

        if (lang == null)
        {
            throw new ArgumentNullException(nameof(lang));
        }

        var keyBuilder = new StringBuilder("group");
        keyBuilder.Append(KeySeparator);
        keyBuilder.Append(project);
        keyBuilder.Append(KeySeparator);
        keyBuilder.Append(group);
        keyBuilder.Append(KeySeparator);
        keyBuilder.Append(lang);

        if (!string.IsNullOrWhiteSpace(format))
        {
            keyBuilder.Append(KeySeparator);
            keyBuilder.Append(format);
        }

        return keyBuilder.ToString();
    }

    /// <summary>
    /// Builds a cache key for a translation project.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="lang">The language code.</param>
    /// <param name="format">Optional format parameter.</param>
    /// <returns>A cache key in the format: "project:project:lang[:format]".</returns>
    /// <exception cref="ArgumentNullException">Thrown when project or lang is null.</exception>
    public static string BuildProjectKey(string project, string lang, string? format = null)
    {
        if (project == null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        if (lang == null)
        {
            throw new ArgumentNullException(nameof(lang));
        }

        var keyBuilder = new StringBuilder("project");
        keyBuilder.Append(KeySeparator);
        keyBuilder.Append(project);
        keyBuilder.Append(KeySeparator);
        keyBuilder.Append(lang);

        if (!string.IsNullOrWhiteSpace(format))
        {
            keyBuilder.Append(KeySeparator);
            keyBuilder.Append(format);
        }

        return keyBuilder.ToString();
    }

    /// <summary>
    /// Builds a cache key for project locales.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <returns>A cache key in the format: "locales:project".</returns>
    /// <exception cref="ArgumentNullException">Thrown when project is null.</exception>
    public static string BuildLocalesKey(string project)
    {
        if (project == null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        return $"locales{KeySeparator}{project}";
    }
}
