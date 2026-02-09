using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Translaas.Models.Responses;

/// <summary>
/// Represents a translation project containing multiple translation groups.
/// </summary>
public class TranslationProject
{
    /// <summary>
    /// Gets or sets the dictionary of translation groups, where the key is the group name
    /// and the value is the translation group.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Groups { get; set; } = [];

    /// <summary>
    /// Gets a translation group by name.
    /// </summary>
    /// <param name="groupName">The group name.</param>
    /// <returns>The translation group, or null if not found.</returns>
    /// <example>
    /// <code>
    /// TranslationProject project = await client.GetProjectAsync("my-project", "en");
    /// TranslationGroup? uiGroup = project.GetGroup("ui");
    /// if (uiGroup != null)
    /// {
    ///     string welcome = uiGroup.GetValue("welcome");
    /// }
    /// </code>
    /// </example>
    public TranslationGroup? GetGroup(string groupName)
    {
        if (Groups.TryGetValue(groupName, out var element))
        {
            // Check if this is a full TranslationGroup JSON (from API) or just entries dictionary (from cache file)
            // Cache files store groups as flat entry dictionaries: { "app.name": "...", "welcome": "..." }
            // API returns full TranslationGroup: { "Project": "...", "Lang": "...", "Entries": { ... } }
            if (element.ValueKind == JsonValueKind.Object)
            {
                // Check if it has "Entries" property (full TranslationGroup from API)
                if (element.TryGetProperty("Entries", out _))
                {
                    // Full TranslationGroup structure - deserialize normally
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<TranslationGroup>(element.GetRawText(), options);
                }
                else
                {
                    // Flat entries dictionary from cache file - wrap it in a TranslationGroup
                    var group = new TranslationGroup();
                    // Deserialize the entries dictionary directly into Entries
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    group.Entries = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(element.GetRawText(), options) ?? new Dictionary<string, JsonElement>();
                    return group;
                }
            }
            
            // Fallback: try to deserialize as TranslationGroup
            var fallbackOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<TranslationGroup>(element.GetRawText(), fallbackOptions);
        }
        return null;
    }
}
