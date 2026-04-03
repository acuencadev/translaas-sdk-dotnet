using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Translaas.Models.Requests;

/// <summary>
/// One missing key reported to <c>POST /sdk/v1/translations/report-missing</c>.
/// </summary>
public sealed class ReportMissingKeyItemRequest
{
    /// <summary>
    /// Translation group key.
    /// </summary>
    [JsonPropertyName("groupKey")]
    public string GroupKey { get; set; } = string.Empty;

    /// <summary>
    /// Entry key.
    /// </summary>
    [JsonPropertyName("entryKey")]
    public string EntryKey { get; set; } = string.Empty;

    /// <summary>
    /// ISO language code.
    /// </summary>
    [JsonPropertyName("languageIsoCode")]
    public string LanguageIsoCode { get; set; } = string.Empty;
}

/// <summary>
/// Request body for reporting missing translation keys.
/// </summary>
public sealed class ReportMissingKeysRequest
{
    /// <summary>
    /// Missing keys to report.
    /// </summary>
    [JsonPropertyName("keys")]
    public List<ReportMissingKeyItemRequest> Keys { get; set; } = [];
}
