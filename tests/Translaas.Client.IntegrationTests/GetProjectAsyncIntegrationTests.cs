using FluentAssertions;

using System.Threading.Tasks;

using Translaas.Client;
using Translaas.Models.Errors;

using Xunit;

namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Integration tests for GetProjectAsync method.
/// These tests require a running development API and TRANSLAAS_API_KEY environment variable.
/// </summary>
public class GetProjectAsyncIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetProjectAsync_ShouldReturnTranslationProject_WhenProjectExists()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        // Note: These values should match your development API test data
        var project = "test-project";
        var lang = "en";

        // Act
        var result = await Client.GetProjectAsync(project, lang);

        // Assert
        result.Should().NotBeNull();
        result.Groups.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProjectAsync_ShouldReturnTranslationProject_WithFormat()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var project = "test-project";
        var lang = "en";
        var format = "json";

        // Act
        var result = await Client.GetProjectAsync(project, lang, format);

        // Assert
        result.Should().NotBeNull();
        result.Groups.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProjectAsync_ShouldThrowTranslaasApiException_WhenProjectNotFound()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var project = "nonexistent-project";
        var lang = "en";

        // Act & Assert
        await Assert.ThrowsAsync<TranslaasApiException>(
            () => Client.GetProjectAsync(project, lang));
    }

    [Fact]
    public async Task GetProjectAsync_ShouldContainMultipleGroups_WhenProjectHasMultipleGroups()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var project = "test-project";
        var lang = "en";

        // Act
        var result = await Client.GetProjectAsync(project, lang);

        // Assert
        result.Should().NotBeNull();
        result.Groups.Should().NotBeEmpty();
        
        // Verify we can access groups
        foreach (var groupName in result.Groups.Keys)
        {
            var group = result.GetGroup(groupName);
            group.Should().NotBeNull();
            group.Entries.Should().NotBeEmpty();
        }
    }
}
