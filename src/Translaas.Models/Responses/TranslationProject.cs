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
            return JsonSerializer.Deserialize<TranslationGroup>(element.GetRawText());
        }
        return null;
    }
}
