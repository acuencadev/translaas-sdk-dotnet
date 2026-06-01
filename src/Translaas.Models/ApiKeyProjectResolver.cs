using System.Linq;
using System.Text.Json;

using Translaas.Models.Errors;
using Translaas.Models.Responses;

namespace Translaas.Models;

/// <summary>
/// Resolves default project ids from validate API key responses.
/// </summary>
public static class ApiKeyProjectResolver
{
    /// <summary>
    /// Resolves the effective default project id when the caller did not configure one explicitly.
    /// </summary>
    public static string ResolveDefaultProjectId(string? configuredProjectId, ValidateApiKeyResponse validate)
    {
        if (!string.IsNullOrWhiteSpace(configuredProjectId))
        {
            return configuredProjectId.Trim();
        }

        var projectIds = validate.ProjectIds ?? [];
        if (projectIds.Count == 0)
        {
            throw new TranslaasConfigurationException(
                "Tenant-level API key requires DefaultProjectId in SDK configuration.");
        }

        var fromValidate = ReadJsonUlid(validate.DefaultProjectId)
            ?? ReadJsonUlid(validate.ProjectId)
            ?? projectIds.FirstOrDefault()?.Trim();

        if (string.IsNullOrWhiteSpace(fromValidate))
        {
            throw new TranslaasConfigurationException(
                "Could not resolve a default project from the validate API key response.");
        }

        return fromValidate;
    }

    private static string? ReadJsonUlid(JsonElement? element)
    {
        if (element is null || element.Value.ValueKind == JsonValueKind.Null || element.Value.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        return element.Value.ValueKind == JsonValueKind.String
            ? element.Value.GetString()
            : element.Value.ToString();
    }
}
