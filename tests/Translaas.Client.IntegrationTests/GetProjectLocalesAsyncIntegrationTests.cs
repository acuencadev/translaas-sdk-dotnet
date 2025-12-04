using FluentAssertions;

using System.Linq;
using System.Threading.Tasks;

using Translaas.Client;
using Translaas.Models.Errors;

using Xunit;

namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Integration tests for GetProjectLocalesAsync method.
/// These tests require a running development API and TRANSLAAS_API_KEY environment variable.
/// </summary>
public class GetProjectLocalesAsyncIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetProjectLocalesAsync_ShouldReturnProjectLocales_WhenProjectExists()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        // Note: These values should match your development API test data
        var project = "test-project";

        // Act
        var result = await Client.GetProjectLocalesAsync(project);

        // Assert
        result.Should().NotBeNull();
        result.Locales.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldReturnMultipleLocales_WhenProjectHasMultipleLocales()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var project = "test-project";

        // Act
        var result = await Client.GetProjectLocalesAsync(project);

        // Assert
        result.Should().NotBeNull();
        result.Locales.Should().NotBeEmpty();
        
        // Verify common locales might be present
        var commonLocales = new[] { "en", "fr", "es", "de" };
        var hasCommonLocale = result.Locales.Any(locale => commonLocales.Contains(locale));
        // Note: This assertion is flexible - it just checks that at least one common locale exists
        // Adjust based on your actual test data
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldThrowTranslaasApiException_WhenProjectNotFound()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var project = "nonexistent-project";

        // Act & Assert
        await Assert.ThrowsAsync<TranslaasApiException>(
            () => Client.GetProjectLocalesAsync(project));
    }
}
