using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Translaas.Models.Responses;

/// <summary>
/// Represents a translation group containing multiple translation entries.
/// </summary>
public class TranslationGroup
{
    /// <summary>
    /// Gets or sets the dictionary of translation entries, where the key is the entry identifier
    /// and the value is the translated text.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Entries { get; set; } = new Dictionary<string, JsonElement>();

    /// <summary>
    /// Gets the translation value for a specific entry key as a string.
    /// </summary>
    /// <param name="key">The entry key.</param>
    /// <returns>The translated text, or null if not found.</returns>
    public string? GetValue(string key)
    {
        if (Entries.TryGetValue(key, out var element))
        {
            return element.GetString();
        }
        return null;
    }
}
