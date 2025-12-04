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
        result.Entries.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowTranslaasApiException_WhenGroupNotFound()
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

        // Act & Assert
        await Assert.ThrowsAsync<Translaas.Models.Errors.TranslaasApiException>(
            () => Client.GetGroupAsync(project, group, lang));
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowTranslaasApiException_WhenProjectNotFound()
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

        // Act & Assert
        await Assert.ThrowsAsync<Translaas.Models.Errors.TranslaasApiException>(
            () => Client.GetGroupAsync(project, group, lang));
    }
}
