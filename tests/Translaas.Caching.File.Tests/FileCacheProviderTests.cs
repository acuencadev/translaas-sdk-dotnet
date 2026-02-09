using System.Text.Json;

using FluentAssertions;
using Translaas.Caching.File.Models;
using Translaas.Models.Responses;

namespace Translaas.Caching.File.Tests;

public class FileCacheProviderTests : IDisposable
{
    private readonly string _testCacheDirectory;
    private readonly FileCacheProvider _provider;

    public FileCacheProviderTests()
    {
        _testCacheDirectory = Path.Combine(Path.GetTempPath(), $"translaas-test-{Guid.NewGuid()}");
        _provider = new FileCacheProvider(_testCacheDirectory);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testCacheDirectory))
        {
            Directory.Delete(_testCacheDirectory, recursive: true);
        }
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithOptions_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        OfflineCacheOptions? options = null;

        // Act
        var act = () => new FileCacheProvider(options!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithDirectory_ThrowsArgumentException_WhenDirectoryIsNull()
    {
        // Arrange
        string? directory = null;

        // Act
        var act = () => new FileCacheProvider(directory!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("cacheDirectory");
    }

    [Fact]
    public void Constructor_WithDirectory_ThrowsArgumentException_WhenDirectoryIsEmpty()
    {
        // Act
        var act = () => new FileCacheProvider(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("cacheDirectory");
    }

    [Fact]
    public void Constructor_WithDirectory_ThrowsArgumentException_WhenDirectoryIsWhitespace()
    {
        // Act
        var act = () => new FileCacheProvider("   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("cacheDirectory");
    }

    [Fact]
    public void Constructor_WithOptions_CreatesProvider()
    {
        // Arrange
        var options = new OfflineCacheOptions
        {
            CacheDirectory = _testCacheDirectory
        };

        // Act
        var provider = new FileCacheProvider(options);

        // Assert
        provider.Should().NotBeNull();
    }

    #endregion

    #region GetProjectAsync Tests

    [Fact]
    public async Task GetProjectAsync_ThrowsArgumentException_WhenProjectIsNull()
    {
        // Act
        var act = () => _provider.GetProjectAsync(null!, "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("project");
    }

    [Fact]
    public async Task GetProjectAsync_ThrowsArgumentException_WhenLanguageIsNull()
    {
        // Act
        var act = () => _provider.GetProjectAsync("test-project", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("lang");
    }

    [Fact]
    public async Task GetProjectAsync_ReturnsNull_WhenNotCached()
    {
        // Act
        var result = await _provider.GetProjectAsync("non-existent", "en");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProjectAsync_ReturnsCachedProject_WhenExists()
    {
        // Arrange
        var project = CreateTestProject();
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Act
        var result = await _provider.GetProjectAsync("test-project", "en");

        // Assert
        result.Should().NotBeNull();
        result!.Groups.Should().ContainKey("common");
    }

    [Fact]
    public async Task GetProjectAsync_SupportsMultipleLanguages()
    {
        // Arrange
        var projectEn = CreateTestProject("Welcome");
        var projectEs = CreateTestProject("Bienvenido");

        await _provider.SaveProjectAsync("test-project", "en", projectEn);
        await _provider.SaveProjectAsync("test-project", "es", projectEs);

        // Act
        var resultEn = await _provider.GetProjectAsync("test-project", "en");
        var resultEs = await _provider.GetProjectAsync("test-project", "es");

        // Assert
        resultEn.Should().NotBeNull();
        resultEs.Should().NotBeNull();
    }

    #endregion

    #region GetGroupAsync Tests

    [Fact]
    public async Task GetGroupAsync_ThrowsArgumentException_WhenProjectIsNull()
    {
        // Act
        var act = () => _provider.GetGroupAsync(null!, "common", "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("project");
    }

    [Fact]
    public async Task GetGroupAsync_ThrowsArgumentException_WhenGroupIsNull()
    {
        // Act
        var act = () => _provider.GetGroupAsync("test-project", null!, "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("group");
    }

    [Fact]
    public async Task GetGroupAsync_ThrowsArgumentException_WhenLanguageIsNull()
    {
        // Act
        var act = () => _provider.GetGroupAsync("test-project", "common", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("lang");
    }

    [Fact]
    public async Task GetGroupAsync_ReturnsNull_WhenProjectNotCached()
    {
        // Act
        var result = await _provider.GetGroupAsync("non-existent", "common", "en");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetGroupAsync_ReturnsNull_WhenGroupNotFound()
    {
        // Arrange
        var project = CreateTestProject();
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Act
        var result = await _provider.GetGroupAsync("test-project", "non-existent-group", "en");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetGroupAsync_ReturnsCachedGroup_WhenExists()
    {
        // Arrange
        var project = CreateTestProject();
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Act
        var result = await _provider.GetGroupAsync("test-project", "common", "en");

        // Assert
        result.Should().NotBeNull();
        result!.GetValue("welcome").Should().NotBeNull();
    }

    #endregion

    #region GetProjectLocalesAsync Tests

    [Fact]
    public async Task GetProjectLocalesAsync_ThrowsArgumentException_WhenProjectIsNull()
    {
        // Act
        var act = () => _provider.GetProjectLocalesAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("project");
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ReturnsNull_WhenNotCached()
    {
        // Act
        var result = await _provider.GetProjectLocalesAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ReturnsCachedLocales_WhenExists()
    {
        // Arrange
        var locales = new ProjectLocales { Locales = ["en", "es", "fr"] };
        await _provider.SaveProjectLocalesAsync("test-project", locales);

        // Act
        var result = await _provider.GetProjectLocalesAsync("test-project");

        // Assert
        result.Should().NotBeNull();
        result!.Locales.Should().BeEquivalentTo(["en", "es", "fr"]);
    }

    #endregion

    #region SaveProjectAsync Tests

    [Fact]
    public async Task SaveProjectAsync_ThrowsArgumentException_WhenProjectIsNull()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var act = () => _provider.SaveProjectAsync(null!, "en", project);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("project");
    }

    [Fact]
    public async Task SaveProjectAsync_ThrowsArgumentException_WhenLanguageIsNull()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var act = () => _provider.SaveProjectAsync("test-project", null!, project);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("lang");
    }

    [Fact]
    public async Task SaveProjectAsync_ThrowsArgumentNullException_WhenDataIsNull()
    {
        // Act
        var act = () => _provider.SaveProjectAsync("test-project", "en", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("data");
    }

    [Fact]
    public async Task SaveProjectAsync_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Assert
        Directory.Exists(_testCacheDirectory).Should().BeTrue();
    }

    [Fact]
    public async Task SaveProjectAsync_UpdatesManifest()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Assert
        var manifest = await _provider.GetManifestAsync();
        manifest.Projects.Should().ContainKey("test-project");
        manifest.Projects["test-project"].Languages.Should().Contain("en");
        manifest.Projects["test-project"].Status.Should().Be(CacheSyncStatus.Synced);
    }

    [Fact]
    public async Task SaveProjectAsync_OverwritesExistingCache()
    {
        // Arrange
        var project1 = CreateTestProject("First");
        var project2 = CreateTestProject("Second");

        await _provider.SaveProjectAsync("test-project", "en", project1);
        await _provider.SaveProjectAsync("test-project", "en", project2);

        // Act
        var result = await _provider.GetProjectAsync("test-project", "en");

        // Assert - should have the second version
        result.Should().NotBeNull();
    }

    #endregion

    #region SaveProjectLocalesAsync Tests

    [Fact]
    public async Task SaveProjectLocalesAsync_ThrowsArgumentException_WhenProjectIsNull()
    {
        // Arrange
        var locales = new ProjectLocales { Locales = ["en"] };

        // Act
        var act = () => _provider.SaveProjectLocalesAsync(null!, locales);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("project");
    }

    [Fact]
    public async Task SaveProjectLocalesAsync_ThrowsArgumentNullException_WhenLocalesIsNull()
    {
        // Act
        var act = () => _provider.SaveProjectLocalesAsync("test-project", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("locales");
    }

    #endregion

    #region IsCachedAsync Tests

    [Fact]
    public async Task IsCachedAsync_ThrowsArgumentException_WhenProjectIsNull()
    {
        // Act
        var act = () => _provider.IsCachedAsync(null!, "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("project");
    }

    [Fact]
    public async Task IsCachedAsync_ThrowsArgumentException_WhenLanguageIsNull()
    {
        // Act
        var act = () => _provider.IsCachedAsync("test-project", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("lang");
    }

    [Fact]
    public async Task IsCachedAsync_ReturnsFalse_WhenNotCached()
    {
        // Act
        var result = await _provider.IsCachedAsync("non-existent", "en");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsCachedAsync_ReturnsTrue_WhenCached()
    {
        // Arrange
        var project = CreateTestProject();
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Act
        var result = await _provider.IsCachedAsync("test-project", "en");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region ClearAllAsync Tests

    [Fact]
    public async Task ClearAllAsync_RemovesCacheDirectory()
    {
        // Arrange
        var project = CreateTestProject();
        await _provider.SaveProjectAsync("test-project", "en", project);
        Directory.Exists(_testCacheDirectory).Should().BeTrue();

        // Act
        await _provider.ClearAllAsync();

        // Assert
        Directory.Exists(_testCacheDirectory).Should().BeFalse();
    }

    [Fact]
    public async Task ClearAllAsync_DoesNotThrow_WhenDirectoryDoesNotExist()
    {
        // Act
        var act = () => _provider.ClearAllAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ClearProjectAsync Tests

    [Fact]
    public async Task ClearProjectAsync_ThrowsArgumentException_WhenProjectIsNull()
    {
        // Act
        var act = () => _provider.ClearProjectAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("project");
    }

    [Fact]
    public async Task ClearProjectAsync_RemovesProjectFromCache()
    {
        // Arrange
        var project = CreateTestProject();
        await _provider.SaveProjectAsync("test-project", "en", project);
        (await _provider.IsCachedAsync("test-project", "en")).Should().BeTrue();

        // Act
        await _provider.ClearProjectAsync("test-project");

        // Assert
        (await _provider.IsCachedAsync("test-project", "en")).Should().BeFalse();
    }

    [Fact]
    public async Task ClearProjectAsync_RemovesProjectFromManifest()
    {
        // Arrange
        var project = CreateTestProject();
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Act
        await _provider.ClearProjectAsync("test-project");

        // Assert
        var manifest = await _provider.GetManifestAsync();
        manifest.Projects.Should().NotContainKey("test-project");
    }

    [Fact]
    public async Task ClearProjectAsync_DoesNotAffectOtherProjects()
    {
        // Arrange
        var project1 = CreateTestProject();
        var project2 = CreateTestProject();
        await _provider.SaveProjectAsync("project-1", "en", project1);
        await _provider.SaveProjectAsync("project-2", "en", project2);

        // Act
        await _provider.ClearProjectAsync("project-1");

        // Assert
        (await _provider.IsCachedAsync("project-1", "en")).Should().BeFalse();
        (await _provider.IsCachedAsync("project-2", "en")).Should().BeTrue();
    }

    #endregion

    #region GetManifestAsync Tests

    [Fact]
    public async Task GetManifestAsync_ReturnsNewManifest_WhenNotExists()
    {
        // Act
        var manifest = await _provider.GetManifestAsync();

        // Assert
        manifest.Should().NotBeNull();
        manifest.Version.Should().Be(CacheManifest.CurrentVersion);
        manifest.Projects.Should().BeEmpty();
    }

    [Fact]
    public async Task GetManifestAsync_ReturnsExistingManifest_WhenExists()
    {
        // Arrange
        var project = CreateTestProject();
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Act
        var manifest = await _provider.GetManifestAsync();

        // Assert
        manifest.Should().NotBeNull();
        manifest.Projects.Should().ContainKey("test-project");
    }

    #endregion

    #region Helper Methods

    private static TranslationProject CreateTestProject(string welcomeValue = "Welcome")
    {
        var project = new TranslationProject();
        var groupJson = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            { "welcome", welcomeValue },
            { "goodbye", "Goodbye" }
        });
        project.Groups["common"] = JsonDocument.Parse(groupJson).RootElement;
        return project;
    }

    #endregion
}
