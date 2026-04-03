using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Translaas.Models.Responses;

/// <summary>
/// Custom JSON converter for TranslationGroup that handles both full structure (with "Entries" property)
/// and flat dictionary structure (where the root object is the entries dictionary).
/// </summary>
internal sealed class TranslationGroupJsonConverter : JsonConverter<TranslationGroup>
{
    public override TranslationGroup? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var group = new TranslationGroup();
        var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // Check if this is a full TranslationGroup structure (has "Entries", "Project", "Lang", etc.)
        if (root.TryGetProperty("Entries", out var entriesElement))
        {
            // Full structure - deserialize normally
            group.Project = root.TryGetProperty("Project", out var projectElement) ? projectElement.GetString() : null;
            group.Lang = root.TryGetProperty("Lang", out var langElement) ? langElement.GetString() : null;
            group.Version = root.TryGetProperty("Version", out var versionElement) ? versionElement : null;
            group.GeneratedAt = root.TryGetProperty("GeneratedAt", out var generatedAtElement) && generatedAtElement.ValueKind != JsonValueKind.Null
                ? generatedAtElement.GetDateTimeOffset()
                : null;
            group.Entries = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(entriesElement.GetRawText(), options) ?? new Dictionary<string, JsonElement>();
            TryReadEntryContext(root, group, options);
        }
        else
        {
            // Flat dictionary structure - treat the root object as the entries dictionary
            group.Entries = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(root.GetRawText(), options) ?? new Dictionary<string, JsonElement>();
            TryReadEntryContext(root, group, options);
        }

        return group;
    }

    private static void TryReadEntryContext(JsonElement root, TranslationGroup group, JsonSerializerOptions options)
    {
        foreach (var name in new[] { "entryContext", "EntryContext" })
        {
            if (root.TryGetProperty(name, out var entryContextElement) && entryContextElement.ValueKind == JsonValueKind.Object)
            {
                group.EntryContext = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(entryContextElement.GetRawText(), options) ?? [];
                return;
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, TranslationGroup value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        if (value.Project != null)
        {
            writer.WriteString("Project", value.Project);
        }
        
        if (value.Lang != null)
        {
            writer.WriteString("Lang", value.Lang);
        }
        
        if (value.Version.HasValue)
        {
            writer.WritePropertyName("Version");
            JsonSerializer.Serialize(writer, value.Version.Value, options);
        }
        
        if (value.GeneratedAt.HasValue)
        {
            writer.WriteString("GeneratedAt", value.GeneratedAt.Value);
        }

        if (value.EntryContext is { Count: > 0 })
        {
            writer.WritePropertyName("entryContext");
            JsonSerializer.Serialize(writer, value.EntryContext, options);
        }

        writer.WritePropertyName("Entries");
        JsonSerializer.Serialize(writer, value.Entries, options);

        writer.WriteEndObject();
    }
}

/// <summary>
/// Represents a translation group containing multiple translation entries.
/// </summary>
[JsonConverter(typeof(TranslationGroupJsonConverter))]
public class TranslationGroup
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    [JsonPropertyName("Project")]
    public string? Project { get; set; }

    /// <summary>
    /// Gets or sets the language code.
    /// </summary>
    [JsonPropertyName("Lang")]
    public string? Lang { get; set; }

    /// <summary>
    /// Gets or sets the version (can be a string or number).
    /// </summary>
    [JsonPropertyName("Version")]
    public JsonElement? Version { get; set; }

    /// <summary>
    /// Gets or sets the generation timestamp.
    /// </summary>
    [JsonPropertyName("GeneratedAt")]
    public DateTimeOffset? GeneratedAt { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of translation entries, where the key is the entry identifier
    /// and the value is either a translated text string or a plural form dictionary.
    /// This property deserializes from the "Entries" property in the JSON response.
    /// </summary>
    [JsonPropertyName("Entries")]
    public Dictionary<string, JsonElement> Entries { get; set; } = [];

    /// <summary>
    /// Optional per-entry context map when <c>includeContext</c> is enabled on the API.
    /// </summary>
    [JsonPropertyName("entryContext")]
    public Dictionary<string, JsonElement>? EntryContext { get; set; }

    /// <summary>
    /// Gets the translation value for a specific entry key as a string.
    /// For entries with plural forms, this returns null. Use <see cref="GetPluralForms"/> to get plural forms.
    /// </summary>
    /// <param name="key">The entry key.</param>
    /// <returns>The translated text, or null if not found or if the entry has plural forms.</returns>
    /// <example>
    /// <code>
    /// TranslationGroup group = await client.GetGroupAsync("my-project", "ui", "en");
    /// string welcome = group.GetValue("welcome"); // Returns "Welcome" or null if not found
    /// </code>
    /// </example>
    public string? GetValue(string key)
    {
        if (Entries.TryGetValue(key, out var element))
        {
            // If the element is a string, return it
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }

            // If the element is an object, it's a plural form dictionary
            // Return null to indicate this entry has plural forms
            if (element.ValueKind == JsonValueKind.Object)
            {
                return null;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the plural forms dictionary for a specific entry key.
    /// </summary>
    /// <param name="key">The entry key.</param>
    /// <returns>A dictionary mapping plural categories to translated text, or null if not found or if the entry is not a plural form entry.</returns>
    /// <example>
    /// <code>
    /// TranslationGroup group = await client.GetGroupAsync("my-project", "messages", "en");
    /// var pluralForms = group.GetPluralForms("item");
    /// if (pluralForms != null)
    /// {
    ///     string singular = pluralForms[PluralCategory.One]; // "1 item"
    ///     string plural = pluralForms[PluralCategory.Other]; // "5 items"
    /// }
    /// </code>
    /// </example>
    public Dictionary<PluralCategory, string>? GetPluralForms(string key)
    {
        if (!Entries.TryGetValue(key, out var element))
        {
            return null;
        }

        // If the element is an object, it's a plural form dictionary
        if (element.ValueKind == JsonValueKind.Object)
        {
            var pluralForms = element.EnumerateObject()
                .Select(p =>
                {
                    if (Enum.TryParse<PluralCategory>(p.Name, ignoreCase: true, out var category))
                    {
                        var value = p.Value.GetString();
                        return value != null ? new { Category = category, Value = value } : null;
                    }
                    return null;
                })
                .Where(p => p != null)
                .ToDictionary(p => p!.Category, p => p!.Value);

            return pluralForms.Count > 0 ? pluralForms : null;
        }

        // If the element is a string, this entry doesn't have plural forms
        return null;
    }

    /// <summary>
    /// Checks if an entry has plural forms.
    /// </summary>
    /// <param name="key">The entry key.</param>
    /// <returns>True if the entry has plural forms, false otherwise.</returns>
    /// <example>
    /// <code>
    /// TranslationGroup group = await client.GetGroupAsync("my-project", "messages", "en");
    /// if (group.HasPluralForms("item"))
    /// {
    ///     var pluralForms = group.GetPluralForms("item");
    /// }
    /// </code>
    /// </example>
    public bool HasPluralForms(string key)
    {
        if (!Entries.TryGetValue(key, out var element))
        {
            return false;
        }

        return element.ValueKind == JsonValueKind.Object;
    }

    /// <summary>
    /// Gets a plural form value for a specific entry key and plural category.
    /// </summary>
    /// <param name="key">The entry key.</param>
    /// <param name="category">The plural category.</param>
    /// <returns>The translated text for the specified plural category, or null if not found.</returns>
    /// <example>
    /// <code>
    /// TranslationGroup group = await client.GetGroupAsync("my-project", "messages", "en");
    /// string singular = group.GetPluralForm("item", PluralCategory.One); // "1 item"
    /// string plural = group.GetPluralForm("item", PluralCategory.Other); // "5 items"
    /// </code>
    /// </example>
    public string? GetPluralForm(string key, PluralCategory category)
    {
        var pluralForms = GetPluralForms(key);
        if (pluralForms != null && pluralForms.TryGetValue(category, out var value))
        {
            return value;
        }
        return null;
    }
}
