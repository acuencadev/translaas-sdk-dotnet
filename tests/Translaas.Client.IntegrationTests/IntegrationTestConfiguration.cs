using System;

namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Configuration for integration tests.
/// Reads configuration from environment variables.
/// </summary>
public class IntegrationTestConfiguration
{
    /// <summary>
    /// Gets the API key for the integration tests.
    /// </summary>
    public string ApiKey { get; }

    /// <summary>
    /// Gets the base URL for the integration tests.
    /// </summary>
    public string BaseUrl { get; }

    /// <summary>
    /// Gets a value indicating whether integration tests are enabled.
    /// </summary>
    public bool IsEnabled { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestConfiguration"/> class.
    /// </summary>
    public IntegrationTestConfiguration()
    {
        ApiKey = Environment.GetEnvironmentVariable("TRANSLAAS_API_KEY") ?? string.Empty;
        // Note: Do NOT include /api in the BaseUrl - the client adds /api/ to all endpoints
        BaseUrl = Environment.GetEnvironmentVariable("TRANSLAAS_BASE_URL") ?? "https://sdk-api.translaas.local";
        
        // Integration tests are enabled if API key is provided
        IsEnabled = !string.IsNullOrWhiteSpace(ApiKey);
    }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException(
                "Integration tests are disabled. Set TRANSLAAS_API_KEY environment variable to enable.");
        }

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new InvalidOperationException(
                "TRANSLAAS_BASE_URL environment variable is required when integration tests are enabled.");
        }
    }
}
