using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Translaas.Models.Responses;

/// <summary>
/// Response payload from <c>GET /api/v1/api-keys/validate</c>.
/// </summary>
public sealed class ValidateApiKeyResponse
{
    /// <summary>
    /// Whether the API key is valid.
    /// </summary>
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    /// <summary>
    /// Tenant identifier when returned by the API (shape may be string or object depending on serializer settings).
    /// </summary>
    [JsonPropertyName("tenantId")]
    public JsonElement? TenantId { get; set; }

    /// <summary>
    /// Default project identifier when the key is project-scoped.
    /// </summary>
    [JsonPropertyName("projectId")]
    public JsonElement? ProjectId { get; set; }

    /// <summary>
    /// All project identifiers the API key may access. Empty for tenant-level keys.
    /// </summary>
    [JsonPropertyName("projectIds")]
    public List<string>? ProjectIds { get; set; }

    /// <summary>
    /// Implicit default project for multi-project keys (first associated project).
    /// </summary>
    [JsonPropertyName("defaultProjectId")]
    public JsonElement? DefaultProjectId { get; set; }

    /// <summary>
    /// Integration name when present.
    /// </summary>
    [JsonPropertyName("integrationName")]
    public string? IntegrationName { get; set; }

    /// <summary>
    /// Authentication timestamp when present.
    /// </summary>
    [JsonPropertyName("authenticatedAt")]
    public System.DateTimeOffset? AuthenticatedAt { get; set; }
}
