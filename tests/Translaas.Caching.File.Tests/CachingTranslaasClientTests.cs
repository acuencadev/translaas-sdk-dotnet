using System.Collections.Generic;
using System.Text.Json;

using FluentAssertions;

using Moq;

using Translaas.Caching.File;
using Translaas.Client;
using Translaas.Models.Errors;
using Translaas.Models.Responses;

namespace Translaas.Caching.File.Tests;

public class CachingTranslaasClientTests
{
    private readonly Mock<ITranslaasClient> _mockInnerClient;
    private readonly Mock<IOfflineCacheProvider> _mockCacheProvider;
    private readonly OfflineCacheOptions _options;
    private const string DefaultProjectId = "test-project";

    public CachingTranslaasClientTests()
    {
        _mockInnerClient = new Mock<ITranslaasClient>();
        _mockCacheProvider = new Mock<IOfflineCacheProvider>();
        _options = new OfflineCacheOptions
        {
            Enabled = true,
            FallbackMode = OfflineFallbackMode.CacheFirst,
            DefaultProjectId = DefaultProjectId
        };
    }

    private CachingTranslaasClient CreateClient(OfflineFallbackMode mode = OfflineFallbackMode.CacheFirst)
    {
        _options.FallbackMode = mode;
        return new CachingTranslaasClient(_mockInnerClient.Object, _mockCacheProvider.Object, _options, DefaultProjectId);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenInnerClientIsNull()
    {
        // Act
        var act = () => new CachingTranslaasClient(null!, _mockCacheProvider.Object, _options, DefaultProjectId);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("innerClient");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCacheProviderIsNull()
    {
        // Act
        var act = () => new CachingTranslaasClient(_mockInnerClient.Object, null!, _options, DefaultProjectId);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cacheProvider");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Act
        var act = () => new CachingTranslaasClient(_mockInnerClient.Object, _mockCacheProvider.Object, null!, DefaultProjectId);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenProjectIdIsNull()
    {
        // Act
        var act = () => new CachingTranslaasClient(_mockInnerClient.Object, _mockCacheProvider.Object, _options, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("projectId");
    }

    #endregion

    #region GetEntryAsync - CacheFirst Tests

    [Fact]
    public async Task GetEntryAsync_CacheFirst_ReturnsCachedValue_WhenCacheHit()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.CacheFirst);
        var cachedGroup = CreateTranslationGroup("hello", "Hello World");

        _mockCacheProvider
            .Setup(c => c.GetGroupAsync(DefaultProjectId, "common", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedGroup);

        // Act
        var result = await client.GetEntryAsync("common", "hello", "en");

        // Assert
        result.Should().Be("Hello World");
        _mockInnerClient.Verify(c => c.GetEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEntryAsync_CacheFirst_CallsApi_WhenCacheMiss()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.CacheFirst);

        _mockCacheProvider
            .Setup(c => c.GetGroupAsync(DefaultProjectId, "common", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TranslationGroup?)null);

        _mockInnerClient
            .Setup(c => c.GetEntryAsync("common", "hello", "en", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Hello from API");

        // Act
        var result = await client.GetEntryAsync("common", "hello", "en");

        // Assert
        result.Should().Be("Hello from API");
    }

    [Fact]
    public async Task GetEntryAsync_CacheFirst_ThrowsCacheMissException_WhenApiFailsAndNoCacheHit()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.CacheFirst);

        _mockCacheProvider
            .Setup(c => c.GetGroupAsync(DefaultProjectId, "common", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TranslationGroup?)null);

        _mockInnerClient
            .Setup(c => c.GetEntryAsync("common", "hello", "en", null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Net.Http.HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<TranslaasOfflineCacheMissException>(
            () => client.GetEntryAsync("common", "hello", "en"));
    }

    #endregion

    #region GetEntryAsync - ApiFirst Tests

    [Fact]
    public async Task GetEntryAsync_ApiFirst_CallsApiFirst()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.ApiFirst);

        _mockInnerClient
            .Setup(c => c.GetEntryAsync("common", "hello", "en", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Hello from API");

        // Act
        var result = await client.GetEntryAsync("common", "hello", "en");

        // Assert
        result.Should().Be("Hello from API");
        _mockInnerClient.Verify(c => c.GetEntryAsync("common", "hello", "en", null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEntryAsync_ApiFirst_FallsBackToCache_OnApiFailure()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.ApiFirst);
        var cachedGroup = CreateTranslationGroup("hello", "Hello from Cache");

        _mockInnerClient
            .Setup(c => c.GetEntryAsync("common", "hello", "en", null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Net.Http.HttpRequestException("Network error"));

        _mockCacheProvider
            .Setup(c => c.GetGroupAsync(DefaultProjectId, "common", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedGroup);

        // Act
        var result = await client.GetEntryAsync("common", "hello", "en");

        // Assert
        result.Should().Be("Hello from Cache");
    }

    #endregion

    #region GetEntryAsync - CacheOnly Tests

    [Fact]
    public async Task GetEntryAsync_CacheOnly_ReturnsCachedValue_WhenCacheHit()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.CacheOnly);
        var cachedGroup = CreateTranslationGroup("hello", "Hello from Cache");

        _mockCacheProvider
            .Setup(c => c.GetGroupAsync(DefaultProjectId, "common", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedGroup);

        // Act
        var result = await client.GetEntryAsync("common", "hello", "en");

        // Assert
        result.Should().Be("Hello from Cache");
        _mockInnerClient.Verify(c => c.GetEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEntryAsync_CacheOnly_ThrowsCacheMissException_WhenNotInCache()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.CacheOnly);

        _mockCacheProvider
            .Setup(c => c.GetGroupAsync(DefaultProjectId, "common", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TranslationGroup?)null);

        // Act & Assert
        await Assert.ThrowsAsync<TranslaasOfflineCacheMissException>(
            () => client.GetEntryAsync("common", "hello", "en"));
    }

    #endregion

    #region GetEntryAsync - ApiOnlyWithBackup Tests

    [Fact]
    public async Task GetEntryAsync_ApiOnlyWithBackup_AlwaysCallsApi()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.ApiOnlyWithBackup);

        _mockInnerClient
            .Setup(c => c.GetEntryAsync("common", "hello", "en", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Hello from API");

        // Act
        var result = await client.GetEntryAsync("common", "hello", "en");

        // Assert
        result.Should().Be("Hello from API");
        _mockInnerClient.Verify(c => c.GetEntryAsync("common", "hello", "en", null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetProjectAsync - CacheFirst Tests

    [Fact]
    public async Task GetProjectAsync_CacheFirst_ReturnsCachedProject_WhenCacheHit()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.CacheFirst);
        var cachedProject = new TranslationProject();

        _mockCacheProvider
            .Setup(c => c.GetProjectAsync("my-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedProject);

        // Act
        var result = await client.GetProjectAsync("my-project", "en");

        // Assert
        result.Should().BeSameAs(cachedProject);
        _mockInnerClient.Verify(c => c.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetProjectAsync_CacheFirst_CallsApi_WhenCacheMiss()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.CacheFirst);
        var apiProject = new TranslationProject();

        _mockCacheProvider
            .Setup(c => c.GetProjectAsync("my-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TranslationProject?)null);

        _mockInnerClient
            .Setup(c => c.GetProjectAsync("my-project", "en", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiProject);

        // Act
        var result = await client.GetProjectAsync("my-project", "en");

        // Assert
        result.Should().BeSameAs(apiProject);
        _mockCacheProvider.Verify(c => c.SaveProjectAsync("my-project", "en", apiProject, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetProjectAsync - ApiFirst Tests

    [Fact]
    public async Task GetProjectAsync_ApiFirst_CallsApiFirst()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.ApiFirst);
        var apiProject = new TranslationProject();

        _mockInnerClient
            .Setup(c => c.GetProjectAsync("my-project", "en", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiProject);

        // Act
        var result = await client.GetProjectAsync("my-project", "en");

        // Assert
        result.Should().BeSameAs(apiProject);
        _mockCacheProvider.Verify(c => c.SaveProjectAsync("my-project", "en", apiProject, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectAsync_ApiFirst_FallsBackToCache_OnApiFailure()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.ApiFirst);
        var cachedProject = new TranslationProject();

        _mockInnerClient
            .Setup(c => c.GetProjectAsync("my-project", "en", null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Net.Http.HttpRequestException("Network error"));

        _mockCacheProvider
            .Setup(c => c.GetProjectAsync("my-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedProject);

        // Act
        var result = await client.GetProjectAsync("my-project", "en");

        // Assert
        result.Should().BeSameAs(cachedProject);
    }

    #endregion

    #region GetProjectAsync - CacheOnly Tests

    [Fact]
    public async Task GetProjectAsync_CacheOnly_ThrowsCacheMissException_WhenNotInCache()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.CacheOnly);

        _mockCacheProvider
            .Setup(c => c.GetProjectAsync("my-project", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TranslationProject?)null);

        // Act & Assert
        await Assert.ThrowsAsync<TranslaasOfflineCacheMissException>(
            () => client.GetProjectAsync("my-project", "en"));
    }

    #endregion

    #region GetGroupAsync Tests

    [Fact]
    public async Task GetGroupAsync_CacheFirst_ReturnsCachedGroup_WhenCacheHit()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.CacheFirst);
        var cachedGroup = new TranslationGroup();

        _mockCacheProvider
            .Setup(c => c.GetGroupAsync("my-project", "common", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedGroup);

        // Act
        var result = await client.GetGroupAsync("my-project", "common", "en");

        // Assert
        result.Should().BeSameAs(cachedGroup);
    }

    [Fact]
    public async Task GetGroupAsync_ApiFirst_CallsApiFirst()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.ApiFirst);
        var apiGroup = new TranslationGroup();

        _mockInnerClient
            .Setup(c => c.GetGroupAsync("my-project", "common", "en", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiGroup);

        // Act
        var result = await client.GetGroupAsync("my-project", "common", "en");

        // Assert
        result.Should().BeSameAs(apiGroup);
    }

    #endregion

    #region GetProjectLocalesAsync Tests

    [Fact]
    public async Task GetProjectLocalesAsync_CacheFirst_ReturnsCachedLocales_WhenCacheHit()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.CacheFirst);
        var cachedLocales = new ProjectLocales { Locales = new List<string> { "en", "es" } };

        _mockCacheProvider
            .Setup(c => c.GetProjectLocalesAsync("my-project", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedLocales);

        // Act
        var result = await client.GetProjectLocalesAsync("my-project");

        // Assert
        result.Should().BeSameAs(cachedLocales);
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ApiFirst_CallsApiFirst_AndCachesResult()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.ApiFirst);
        var apiLocales = new ProjectLocales { Locales = new List<string> { "en", "es" } };

        _mockInnerClient
            .Setup(c => c.GetProjectLocalesAsync("my-project", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiLocales);

        // Act
        var result = await client.GetProjectLocalesAsync("my-project");

        // Assert
        result.Should().BeSameAs(apiLocales);
        _mockCacheProvider.Verify(c => c.SaveProjectLocalesAsync("my-project", apiLocales, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectLocalesAsync_CacheOnly_ThrowsException_WhenNotInCache()
    {
        // Arrange
        var client = CreateClient(OfflineFallbackMode.CacheOnly);

        _mockCacheProvider
            .Setup(c => c.GetProjectLocalesAsync("my-project", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectLocales?)null);

        // Act & Assert
        await Assert.ThrowsAsync<TranslaasOfflineCacheException>(
            () => client.GetProjectLocalesAsync("my-project"));
    }

    #endregion

    #region Helper Methods

    private static TranslationGroup CreateTranslationGroup(string key, string value)
    {
        var group = new TranslationGroup();
        var json = JsonSerializer.Serialize(value);
        group.Entries[key] = JsonDocument.Parse(json).RootElement;
        return group;
    }

    #endregion
}
