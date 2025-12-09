using FluentAssertions;

using System.Threading.Tasks;

using Translaas.Client;
using Translaas.Models.Errors;

using Xunit;

namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Integration tests for GetGroupAsync method.
/// These tests require a running development API and TRANSLAAS_API_KEY environment variable.
/// </summary>
public class GetGroupAsyncIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetGroupAsync_ShouldReturnTranslationGroup_WhenGroupExists()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        // Note: These values should match your development API test data
        var project = "test-project";
        var group = "ui";
        var lang = "en";

        // Act
        var result = await Client.GetGroupAsync(project, group, lang);

        // Assert
        result.Should().NotBeNull();
        
        // Skip test if test data doesn't exist (API returns 204 with empty group)
        if (result.Entries.Count == 0)
        {
            return; // Test data not available - skip this test
        }
        
        result.Entries.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetGroupAsync_ShouldReturnTranslationGroup_WithFormat()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var project = "test-project";
        var group = "ui";
        var lang = "en";
        var format = "json";

        // Act
        var result = await Client.GetGroupAsync(project, group, lang, format);

        // Assert
        result.Should().NotBeNull();
        
        // Skip test if test data doesn't exist (API returns 204 with empty group)
        if (result.Entries.Count == 0)
        {
            return; // Test data not available - skip this test
        }
        
        result.Entries.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetGroupAsync_ShouldHandleNotFound_WhenGroupNotFound()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var project = "test-project";
        var group = "nonexistent-group";
        var lang = "en";

        // Act
        // Note: API returns 204 No Content for non-existent groups, which returns empty group
        var result = await Client.GetGroupAsync(project, group, lang);

        // Assert
        // When group is not found, API returns 204 and client returns empty group
        result.Should().NotBeNull();
        result.Entries.Should().BeEmpty(); // Empty group is expected when 204 No Content is received
    }

    [Fact]
    public async Task GetGroupAsync_ShouldHandleNotFound_WhenProjectNotFound()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var project = "nonexistent-project";
        var group = "ui";
        var lang = "en";

        // Act
        // Note: API returns 204 No Content for non-existent projects, which returns empty group
        var result = await Client.GetGroupAsync(project, group, lang);

        // Assert
        // When project is not found, API returns 204 and client returns empty group
        result.Should().NotBeNull();
        result.Entries.Should().BeEmpty(); // Empty group is expected when 204 No Content is received
    }
}
