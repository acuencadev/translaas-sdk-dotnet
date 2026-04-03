using FluentAssertions;

using Moq;
using Translaas.Caching.File.Models;
using Translaas.Client;
using Translaas.Models.Responses;

namespace Translaas.Caching.File.Tests;

public class OfflineCacheSyncServiceTests : IDisposable
{
    private readonly Mock<ITranslaasClient> _mockClient;
    private readonly Mock<IOfflineCacheProvider> _mockCacheProvider;
    private readonly OfflineCacheOptions _options;
    private readonly OfflineCacheSyncService _service;

    public OfflineCacheSyncServiceTests()
    {
        _mockClient = new Mock<ITranslaasClient>();
        _mockCacheProvider = new Mock<IOfflineCacheProvider>();
        _options = new OfflineCacheOptions
        {
            Enabled = true,
            Projects = ["test-project"],
            Languages = ["en", "es"]
        };
        _service = new OfflineCacheSyncService(_mockClient.Object, _mockCacheProvider.Object, _options);
    }

    public void Dispose()
    {
        _service.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenClientIsNull()
    {
        // Act
        var act = () => new OfflineCacheSyncService(null!, _mockCacheProvider.Object, _options);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("client");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCacheProviderIsNull()
    {
        // Act
        var act = () => new OfflineCacheSyncService(_mockClient.Object, null!, _options);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cacheProvider");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Act
        var act = () => new OfflineCacheSyncService(_mockClient.Object, _mockCacheProvider.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    #endregion

    #region SyncProjectAsync Tests

    [Fact]
    public async Task SyncProjectAsync_ThrowsArgumentException_WhenProjectIsNull()
    {
        // Act
        var act = () => _service.SyncProjectAsync(null!, "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("project");
    }

    [Fact]
    public async Task SyncProjectAsync_ThrowsArgumentException_WhenLanguageIsNull()
    {
        // Act
        var act = () => _service.SyncProjectAsync("test-project", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("lang");
    }

    [Fact]
    public async Task SyncProjectAsync_FetchesAndCachesProject()
    {
        // Arrange
        var project = new TranslationProject();
        _mockClient
            .Setup(c => c.GetProjectAsync("test-project", "en", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        await _service.SyncProjectAsync("test-project", "en");

        // Assert
        _mockClient.Verify(c => c.GetProjectAsync("test-project", "en", null, null, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheProvider.Verify(c => c.SaveProjectAsync("test-project", "en", project, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncProjectAsync_RaisesSyncCompletedEvent_OnSuccess()
    {
        // Arrange
        var project = new TranslationProject();
        _mockClient
            .Setup(c => c.GetProjectAsync("test-project", "en", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        CacheSyncEventArgs? eventArgs = null;
        _service.SyncCompleted += (sender, e) => eventArgs = e;

        // Act
        await _service.SyncProjectAsync("test-project", "en");

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.Project.Should().Be("test-project");
        eventArgs.Language.Should().Be("en");
    }

    [Fact]
    public async Task SyncProjectAsync_RaisesSyncFailedEvent_OnError()
    {
        // Arrange
        var exception = new Exception("Test error");
        _mockClient
            .Setup(c => c.GetProjectAsync("test-project", "en", null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        CacheSyncErrorEventArgs? eventArgs = null;
        _service.SyncFailed += (sender, e) => eventArgs = e;

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.SyncProjectAsync("test-project", "en"));

        eventArgs.Should().NotBeNull();
        eventArgs!.Project.Should().Be("test-project");
        eventArgs.Language.Should().Be("en");
        eventArgs.Exception.Should().BeSameAs(exception);
    }

    #endregion

    #region SyncProjectAllLanguagesAsync Tests

    [Fact]
    public async Task SyncProjectAllLanguagesAsync_ThrowsArgumentException_WhenProjectIsNull()
    {
        // Act
        var act = () => _service.SyncProjectAllLanguagesAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("project");
    }

    [Fact]
    public async Task SyncProjectAllLanguagesAsync_FetchesLocalesAndProjects()
    {
        // Arrange
        var locales = new ProjectLocales { Locales = ["en", "es", "fr"] };
        _mockClient
            .Setup(c => c.GetProjectLocalesAsync("test-project", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(locales);

        var project = new TranslationProject();
        _mockClient
            .Setup(c => c.GetProjectAsync("test-project", It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        await _service.SyncProjectAllLanguagesAsync("test-project");

        // Assert - Should only sync "en" and "es" since those are in options.Languages
        _mockClient.Verify(c => c.GetProjectLocalesAsync("test-project", null, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheProvider.Verify(c => c.SaveProjectLocalesAsync("test-project", locales, It.IsAny<CancellationToken>()), Times.Once);
        _mockClient.Verify(c => c.GetProjectAsync("test-project", "en", null, null, It.IsAny<CancellationToken>()), Times.Once);
        _mockClient.Verify(c => c.GetProjectAsync("test-project", "es", null, null, It.IsAny<CancellationToken>()), Times.Once);
        _mockClient.Verify(c => c.GetProjectAsync("test-project", "fr", null, null, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncProjectAllLanguagesAsync_SyncsAllLanguages_WhenNoLanguagesConfigured()
    {
        // Arrange
        var optionsWithNoLanguages = new OfflineCacheOptions
        {
            Enabled = true,
            Projects = ["test-project"],
            Languages = [] // Empty = all languages
        };
        using var service = new OfflineCacheSyncService(_mockClient.Object, _mockCacheProvider.Object, optionsWithNoLanguages);

        var locales = new ProjectLocales { Locales = ["en", "es", "fr"] };
        _mockClient
            .Setup(c => c.GetProjectLocalesAsync("test-project", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(locales);

        var project = new TranslationProject();
        _mockClient
            .Setup(c => c.GetProjectAsync("test-project", It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        await service.SyncProjectAllLanguagesAsync("test-project");

        // Assert - Should sync all three languages
        _mockClient.Verify(c => c.GetProjectAsync("test-project", "en", null, null, It.IsAny<CancellationToken>()), Times.Once);
        _mockClient.Verify(c => c.GetProjectAsync("test-project", "es", null, null, It.IsAny<CancellationToken>()), Times.Once);
        _mockClient.Verify(c => c.GetProjectAsync("test-project", "fr", null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region SyncAllAsync Tests

    [Fact]
    public async Task SyncAllAsync_SyncsAllConfiguredProjects()
    {
        // Arrange
        var locales = new ProjectLocales { Locales = ["en", "es"] };
        _mockClient
            .Setup(c => c.GetProjectLocalesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(locales);

        var project = new TranslationProject();
        _mockClient
            .Setup(c => c.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        CacheSyncAllEventArgs? eventArgs = null;
        _service.SyncAllCompleted += (sender, e) => eventArgs = e;

        // Act
        await _service.SyncAllAsync();

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.SyncedProjects.Should().Contain("test-project");
        eventArgs.FailedProjects.Should().BeEmpty();
    }

    [Fact]
    public async Task SyncAllAsync_ContinuesOnError_AndReportsFailedProjects()
    {
        // Arrange
        var optionsWithMultipleProjects = new OfflineCacheOptions
        {
            Enabled = true,
            Projects = ["project-1", "project-2"],
            Languages = ["en"]
        };
        using var service = new OfflineCacheSyncService(_mockClient.Object, _mockCacheProvider.Object, optionsWithMultipleProjects);

        // project-1 fails
        _mockClient
            .Setup(c => c.GetProjectLocalesAsync("project-1", null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));

        // project-2 succeeds
        var locales = new ProjectLocales { Locales = ["en"] };
        _mockClient
            .Setup(c => c.GetProjectLocalesAsync("project-2", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(locales);

        var project = new TranslationProject();
        _mockClient
            .Setup(c => c.GetProjectAsync("project-2", "en", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        CacheSyncAllEventArgs? eventArgs = null;
        service.SyncAllCompleted += (sender, e) => eventArgs = e;

        // Act
        await service.SyncAllAsync();

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.SyncedProjects.Should().Contain("project-2");
        eventArgs.FailedProjects.Should().Contain("project-1");
    }

    #endregion

    #region Background Sync Tests

    [Fact]
    public void IsBackgroundSyncRunning_ReturnsFalse_Initially()
    {
        // Assert
        _service.IsBackgroundSyncRunning.Should().BeFalse();
    }

    [Fact]
    public async Task StartBackgroundSyncAsync_DoesNotStart_WhenAutoSyncDisabled()
    {
        // Arrange
        var optionsWithAutoSyncDisabled = new OfflineCacheOptions
        {
            Enabled = true,
            AutoSync = false
        };
        using var service = new OfflineCacheSyncService(_mockClient.Object, _mockCacheProvider.Object, optionsWithAutoSyncDisabled);

        // Act
        await service.StartBackgroundSyncAsync();

        // Assert
        service.IsBackgroundSyncRunning.Should().BeFalse();
    }

    [Fact]
    public async Task StopBackgroundSyncAsync_DoesNotThrow_WhenNotStarted()
    {
        // Act
        var act = () => _service.StopBackgroundSyncAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion
}
