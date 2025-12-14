using System.Text.Json;

using FluentAssertions;

using Moq;

using Translaas.Caching.File;
using Translaas.Caching.File.Models;
using Translaas.Models.Responses;

namespace Translaas.Caching.File.Tests;

public class HybridCacheProviderTests
{
    private readonly Mock<IOfflineCacheProvider> _mockFileCache;
    private readonly HybridCacheOptions _options;
    private readonly HybridCacheProvider _provider;

    public HybridCacheProviderTests()
    {
        _mockFileCache = new Mock<IOfflineCacheProvider>();
        _options = new HybridCacheOptions
        {
            Enabled = true,
            MemoryCacheExpiration = TimeSpan.FromMinutes(30),
            MaxMemoryCacheEntries = 100
        };
        _provider = new HybridCacheProvider(_mockFileCache.Object, _options);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFileCacheIsNull()
    {
        // Act
        var act = () => new HybridCacheProvider(null!, _options);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("fileCache");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Act
        var act = () => new HybridCacheProvider(_mockFileCache.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    #endregion

    #region GetProjectAsync Tests

    [Fact]
    public async Task GetProjectAsync_ReturnsFromL1_WhenInMemoryCache()
    {
        // Arrange
        var project = CreateTestProject();
        _mockFileCache
            .Setup(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // First call populates L1
        await _provider.GetProjectAsync("test-project", "en");

        // Act - Second call should use L1
        var result = await _provider.GetProjectAsync("test-project", "en");

        // Assert
        result.Should().NotBeNull();
        // File cache should only be called once (first call)
        _mockFileCache.Verify(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectAsync_FallsBackToL2_WhenNotInL1()
    {
        // Arrange
        var project = CreateTestProject();
        _mockFileCache
            .Setup(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        var result = await _provider.GetProjectAsync("test-project", "en");

        // Assert
        result.Should().NotBeNull();
        _mockFileCache.Verify(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectAsync_ReturnsNull_WhenNotInAnyCache()
    {
        // Arrange
        _mockFileCache
            .Setup(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TranslationProject?)null);

        // Act
        var result = await _provider.GetProjectAsync("test-project", "en");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProjectAsync_PopulatesL1_FromL2Hit()
    {
        // Arrange
        var project = CreateTestProject();
        _mockFileCache
            .Setup(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act - First call fetches from L2 and populates L1
        await _provider.GetProjectAsync("test-project", "en");

        // Second call should hit L1
        await _provider.GetProjectAsync("test-project", "en");

        // Third call should still hit L1
        await _provider.GetProjectAsync("test-project", "en");

        // Assert - L2 should only be called once
        _mockFileCache.Verify(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetGroupAsync Tests

    [Fact]
    public async Task GetGroupAsync_ReturnsFromL1_WhenInMemoryCache()
    {
        // Arrange
        var group = CreateTestGroup("hello", "Hello");
        _mockFileCache
            .Setup(c => c.GetGroupAsync("test-project", "common", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        // First call populates L1
        await _provider.GetGroupAsync("test-project", "common", "en");

        // Act - Second call should use L1
        var result = await _provider.GetGroupAsync("test-project", "common", "en");

        // Assert
        result.Should().NotBeNull();
        _mockFileCache.Verify(c => c.GetGroupAsync("test-project", "common", "en", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGroupAsync_FallsBackToL2_WhenNotInL1()
    {
        // Arrange
        var group = CreateTestGroup("hello", "Hello");
        _mockFileCache
            .Setup(c => c.GetGroupAsync("test-project", "common", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        // Act
        var result = await _provider.GetGroupAsync("test-project", "common", "en");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region GetProjectLocalesAsync Tests

    [Fact]
    public async Task GetProjectLocalesAsync_ReturnsFromL1_WhenInMemoryCache()
    {
        // Arrange
        var locales = new ProjectLocales { Locales = new List<string> { "en", "es" } };
        _mockFileCache
            .Setup(c => c.GetProjectLocalesAsync("test-project", It.IsAny<CancellationToken>()))
            .ReturnsAsync(locales);

        // First call populates L1
        await _provider.GetProjectLocalesAsync("test-project");

        // Act - Second call should use L1
        var result = await _provider.GetProjectLocalesAsync("test-project");

        // Assert
        result.Should().NotBeNull();
        result!.Locales.Should().BeEquivalentTo(new[] { "en", "es" });
        _mockFileCache.Verify(c => c.GetProjectLocalesAsync("test-project", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region SaveProjectAsync Tests

    [Fact]
    public async Task SaveProjectAsync_SavesToBothL1AndL2()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Assert - L2 should be called
        _mockFileCache.Verify(c => c.SaveProjectAsync("test-project", "en", project, It.IsAny<CancellationToken>()), Times.Once);

        // L1 should be populated - subsequent get should not call L2
        var result = await _provider.GetProjectAsync("test-project", "en");
        result.Should().NotBeNull();
        _mockFileCache.Verify(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveProjectAsync_CachesGroupsInL1()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Assert - Groups should be cached in L1
        var group = await _provider.GetGroupAsync("test-project", "common", "en");

        // File cache GetGroupAsync should not be called if group was cached from SaveProjectAsync
        _mockFileCache.Verify(c => c.GetGroupAsync("test-project", "common", "en", It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region SaveProjectLocalesAsync Tests

    [Fact]
    public async Task SaveProjectLocalesAsync_SavesToBothL1AndL2()
    {
        // Arrange
        var locales = new ProjectLocales { Locales = new List<string> { "en", "es" } };

        // Act
        await _provider.SaveProjectLocalesAsync("test-project", locales);

        // Assert - L2 should be called
        _mockFileCache.Verify(c => c.SaveProjectLocalesAsync("test-project", locales, It.IsAny<CancellationToken>()), Times.Once);

        // L1 should be populated
        var result = await _provider.GetProjectLocalesAsync("test-project");
        result.Should().NotBeNull();
        _mockFileCache.Verify(c => c.GetProjectLocalesAsync("test-project", It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region IsCachedAsync Tests

    [Fact]
    public async Task IsCachedAsync_ReturnsTrue_WhenInL1()
    {
        // Arrange
        var project = CreateTestProject();
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Act
        var result = await _provider.IsCachedAsync("test-project", "en");

        // Assert
        result.Should().BeTrue();
        _mockFileCache.Verify(c => c.IsCachedAsync("test-project", "en", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IsCachedAsync_ChecksL2_WhenNotInL1()
    {
        // Arrange
        _mockFileCache
            .Setup(c => c.IsCachedAsync("test-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _provider.IsCachedAsync("test-project", "en");

        // Assert
        result.Should().BeTrue();
        _mockFileCache.Verify(c => c.IsCachedAsync("test-project", "en", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ClearAllAsync Tests

    [Fact]
    public async Task ClearAllAsync_ClearsBothL1AndL2()
    {
        // Arrange
        var project = CreateTestProject();
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Act
        await _provider.ClearAllAsync();

        // Assert - L2 should be called
        _mockFileCache.Verify(c => c.ClearAllAsync(It.IsAny<CancellationToken>()), Times.Once);

        // L1 should be empty
        var stats = _provider.GetMemoryCacheStats();
        stats.Projects.Should().Be(0);
        stats.Groups.Should().Be(0);
        stats.Locales.Should().Be(0);
    }

    #endregion

    #region ClearProjectAsync Tests

    [Fact]
    public async Task ClearProjectAsync_ClearsBothL1AndL2ForProject()
    {
        // Arrange
        var project1 = CreateTestProject();
        var project2 = CreateTestProject();
        await _provider.SaveProjectAsync("project-1", "en", project1);
        await _provider.SaveProjectAsync("project-2", "en", project2);

        // Act
        await _provider.ClearProjectAsync("project-1");

        // Assert - L2 should be called for project-1
        _mockFileCache.Verify(c => c.ClearProjectAsync("project-1", It.IsAny<CancellationToken>()), Times.Once);

        // project-1 should be cleared from L1, but project-2 should remain
        _mockFileCache
            .Setup(c => c.GetProjectAsync("project-1", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TranslationProject?)null);

        var result1 = await _provider.GetProjectAsync("project-1", "en");
        result1.Should().BeNull();

        // project-2 should still be in L1 (no L2 call needed)
        var result2 = await _provider.GetProjectAsync("project-2", "en");
        result2.Should().NotBeNull();
    }

    #endregion

    #region ClearMemoryCache Tests

    [Fact]
    public async Task ClearMemoryCache_ClearsOnlyL1()
    {
        // Arrange
        var project = CreateTestProject();
        await _provider.SaveProjectAsync("test-project", "en", project);

        // Act
        _provider.ClearMemoryCache();

        // Assert - L1 should be empty
        var stats = _provider.GetMemoryCacheStats();
        stats.Projects.Should().Be(0);

        // L2 should NOT be cleared
        _mockFileCache.Verify(c => c.ClearAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetMemoryCacheStats Tests

    [Fact]
    public async Task GetMemoryCacheStats_ReturnsCorrectCounts()
    {
        // Arrange
        var project = CreateTestProject();
        var locales = new ProjectLocales { Locales = new List<string> { "en" } };

        await _provider.SaveProjectAsync("test-project", "en", project);
        await _provider.SaveProjectLocalesAsync("test-project", locales);

        // Act
        var stats = _provider.GetMemoryCacheStats();

        // Assert
        stats.Projects.Should().Be(1);
        stats.Groups.Should().BeGreaterThan(0); // Groups are cached from project
        stats.Locales.Should().Be(1);
    }

    #endregion

    #region WarmupAsync Tests

    [Fact]
    public async Task WarmupAsync_PopulatesL1FromL2()
    {
        // Arrange
        var project = CreateTestProject();
        _mockFileCache
            .Setup(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        var result = await _provider.WarmupAsync("test-project", "en");

        // Assert
        result.Should().BeTrue();

        // Subsequent get should not call L2
        await _provider.GetProjectAsync("test-project", "en");
        _mockFileCache.Verify(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WarmupAsync_ReturnsFalse_WhenProjectNotInL2()
    {
        // Arrange
        _mockFileCache
            .Setup(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TranslationProject?)null);

        // Act
        var result = await _provider.WarmupAsync("test-project", "en");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Expiration Tests

    [Fact]
    public async Task L1Cache_ExpiresAfterConfiguredTime()
    {
        // Arrange - Use very short expiration
        var shortExpirationOptions = new HybridCacheOptions
        {
            Enabled = true,
            MemoryCacheExpiration = TimeSpan.FromMilliseconds(50)
        };
        var provider = new HybridCacheProvider(_mockFileCache.Object, shortExpirationOptions);

        var project = CreateTestProject();
        _mockFileCache
            .Setup(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // First call populates L1
        await provider.GetProjectAsync("test-project", "en");

        // Wait for expiration
        await Task.Delay(100);

        // Act - Second call should hit L2 since L1 expired
        await provider.GetProjectAsync("test-project", "en");

        // Assert - L2 should be called twice
        _mockFileCache.Verify(c => c.GetProjectAsync("test-project", "en", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    #endregion

    #region GetManifestAsync Tests

    [Fact]
    public async Task GetManifestAsync_DelegatesToL2()
    {
        // Arrange
        var manifest = new CacheManifest { Version = "1.0" };
        _mockFileCache
            .Setup(c => c.GetManifestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(manifest);

        // Act
        var result = await _provider.GetManifestAsync();

        // Assert
        result.Should().BeSameAs(manifest);
        _mockFileCache.Verify(c => c.GetManifestAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static TranslationProject CreateTestProject()
    {
        var project = new TranslationProject();
        var groupJson = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            { "hello", "Hello" },
            { "world", "World" }
        });
        project.Groups["common"] = JsonDocument.Parse(groupJson).RootElement;
        return project;
    }

    private static TranslationGroup CreateTestGroup(string key, string value)
    {
        var group = new TranslationGroup();
        var json = JsonSerializer.Serialize(value);
        group.Entries[key] = JsonDocument.Parse(json).RootElement;
        return group;
    }

    #endregion
}
