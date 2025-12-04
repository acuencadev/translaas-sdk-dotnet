using FluentAssertions;

using System.Threading.Tasks;

using Translaas.Client;
using Translaas.Models.Errors;

using Xunit;

namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Integration tests for GetEntryAsync method.
/// These tests require a running development API and TRANSLAAS_API_KEY environment variable.
/// </summary>
public class GetEntryAsyncIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetEntryAsync_ShouldReturnTranslation_WhenEntryExists()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        // Note: These values should match your development API test data
        var group = "ui";
        var entry = "button.save";
        var lang = "en";

        // Act
        var result = await Client.GetEntryAsync(group, entry, lang);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEntryAsync_ShouldReturnTranslation_WithPluralization()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var group = "ui";
        var entry = "items.count";
        var lang = "en";
        var number = 5;

        // Act
        var result = await Client.GetEntryAsync(group, entry, lang, number);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEntryAsync_ShouldThrowTranslaasApiException_WhenEntryNotFound()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var group = "nonexistent";
        var entry = "nonexistent.entry";
        var lang = "en";

        // Act & Assert
        await Assert.ThrowsAsync<TranslaasApiException>(
            () => Client.GetEntryAsync(group, entry, lang));
    }

    [Fact]
    public async Task GetEntryAsync_ShouldThrowTranslaasApiException_WhenInvalidApiKey()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var invalidOptions = new TranslaasClientOptions
        {
            ApiKey = "invalid-api-key",
            BaseUrl = Configuration.BaseUrl
        };
        var invalidClient = new TranslaasClient(HttpClient, invalidOptions);

        // Act & Assert
        await Assert.ThrowsAsync<TranslaasApiException>(
            () => invalidClient.GetEntryAsync("ui", "button.save", "en"));
    }
}
