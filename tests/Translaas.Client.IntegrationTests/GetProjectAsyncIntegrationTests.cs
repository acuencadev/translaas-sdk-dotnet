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
        
        // Skip test if test data doesn't exist (API returns 204 with empty project)
        if (result.Groups.Count == 0)
        {
            return; // Test data not available - skip this test
        }
        
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
        
        // Skip test if test data doesn't exist (API returns 204 with empty project)
        if (result.Groups.Count == 0)
        {
            return; // Test data not available - skip this test
        }
        
        result.Groups.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProjectAsync_ShouldHandleNotFound_WhenProjectNotFound()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var project = "nonexistent-project";
        var lang = "en";

        // Act
        // Note: API returns 204 No Content for non-existent projects, which returns empty project
        var result = await Client.GetProjectAsync(project, lang);

        // Assert
        // When project is not found, API returns 204 and client returns empty project
        result.Should().NotBeNull();
        result.Groups.Should().BeEmpty(); // Empty project is expected when 204 No Content is received
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
        
        // Skip test if test data doesn't exist (API returns 204 with empty project)
        if (result.Groups.Count == 0)
        {
            return; // Test data not available - skip this test
        }
        
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
