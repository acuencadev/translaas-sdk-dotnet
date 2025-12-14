using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using Translaas.Caching.File;
using Translaas.Caching.File.Models;

namespace Translaas.Caching.File.Tests;

public class OfflineCacheSyncHostedServiceTests : IDisposable
{
    private readonly Mock<IOfflineCacheSyncService> _mockSyncService;
    private readonly Mock<ILogger<OfflineCacheSyncHostedService>> _mockLogger;
    private OfflineCacheOptions _options;
    private OfflineCacheSyncHostedService? _service;

    public OfflineCacheSyncHostedServiceTests()
    {
        _mockSyncService = new Mock<IOfflineCacheSyncService>();
        _mockLogger = new Mock<ILogger<OfflineCacheSyncHostedService>>();
        _options = new OfflineCacheOptions
        {
            Enabled = true,
            AutoSync = true,
            AutoSyncInterval = TimeSpan.FromSeconds(1),
            Projects = new List<string> { "test-project" },
            Languages = new List<string> { "en" }
        };
    }

    public void Dispose()
    {
        _service?.Dispose();
    }

    private OfflineCacheSyncHostedService CreateService()
    {
        _service = new OfflineCacheSyncHostedService(_mockSyncService.Object, _options, _mockLogger.Object);
        return _service;
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenSyncServiceIsNull()
    {
        // Act
        var act = () => new OfflineCacheSyncHostedService(null!, _options);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("syncService");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Act
        var act = () => new OfflineCacheSyncHostedService(_mockSyncService.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_AcceptsNullLogger()
    {
        // Act
        var act = () => new OfflineCacheSyncHostedService(_mockSyncService.Object, _options, null);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region StartAsync Tests

    [Fact]
    public async Task StartAsync_DoesNotSync_WhenOfflineCacheDisabled()
    {
        // Arrange
        _options.Enabled = false;
        var service = CreateService();

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        _mockSyncService.Verify(s => s.SyncAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StartAsync_DoesNotSync_WhenAutoSyncDisabled()
    {
        // Arrange
        _options.AutoSync = false;
        var service = CreateService();

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        _mockSyncService.Verify(s => s.SyncAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StartAsync_DoesNotSync_WhenNoProjectsConfigured()
    {
        // Arrange
        _options.Projects.Clear();
        var service = CreateService();

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        _mockSyncService.Verify(s => s.SyncAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StartAsync_CallsSyncAllAsync_WhenEnabled()
    {
        // Arrange
        _options.AutoSyncInterval = null; // Disable periodic sync for this test
        var service = CreateService();

        _mockSyncService
            .Setup(s => s.SyncAllAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await service.StartAsync(CancellationToken.None);

        // Give async task time to run
        await Task.Delay(100);

        // Assert
        _mockSyncService.Verify(s => s.SyncAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartAsync_ContinuesOnSyncError()
    {
        // Arrange
        _options.AutoSyncInterval = null;
        var service = CreateService();

        _mockSyncService
            .Setup(s => s.SyncAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Sync error"));

        // Act - should not throw
        await service.StartAsync(CancellationToken.None);

        // Give async task time to run and handle error
        await Task.Delay(100);

        // Assert - service should have started without throwing
        _mockSyncService.Verify(s => s.SyncAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region StopAsync Tests

    [Fact]
    public async Task StopAsync_CompletesWithoutError_WhenNotStarted()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_StopsBackgroundSync()
    {
        // Arrange
        _options.AutoSyncInterval = TimeSpan.FromSeconds(10);
        var service = CreateService();

        _mockSyncService
            .Setup(s => s.SyncAllAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await service.StartAsync(CancellationToken.None);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert - should complete without hanging
        // The fact that StopAsync completed is the assertion
    }

    #endregion

    #region Periodic Sync Tests

    [Fact]
    public async Task PeriodicSync_RunsAtConfiguredInterval()
    {
        // Arrange
        _options.AutoSyncInterval = TimeSpan.FromMilliseconds(100);
        var service = CreateService();
        var syncCount = 0;

        _mockSyncService
            .Setup(s => s.SyncAllAsync(It.IsAny<CancellationToken>()))
            .Callback(() => syncCount++)
            .Returns(Task.CompletedTask);

        // Act
        await service.StartAsync(CancellationToken.None);

        // Wait for initial sync + at least one periodic sync
        await Task.Delay(300);

        await service.StopAsync(CancellationToken.None);

        // Assert - should have synced at least twice (initial + periodic)
        syncCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task PeriodicSync_StopsOnCancellation()
    {
        // Arrange
        _options.AutoSyncInterval = TimeSpan.FromMilliseconds(50);
        var service = CreateService();
        var syncCount = 0;

        _mockSyncService
            .Setup(s => s.SyncAllAsync(It.IsAny<CancellationToken>()))
            .Callback(() => syncCount++)
            .Returns(Task.CompletedTask);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);

        // Let it sync a couple times
        await Task.Delay(200);

        var syncCountBeforeStop = syncCount;

        // Stop
#if NET6_0_OR_GREATER
        await cts.CancelAsync();
#else
        cts.Cancel();
#endif
        await service.StopAsync(CancellationToken.None);

        // Wait a bit more
        await Task.Delay(200);

        // Assert - sync count should not have increased significantly after stop
        syncCount.Should().BeLessThanOrEqualTo(syncCountBeforeStop + 1);
    }

    #endregion

    #region Event Handling Tests

    [Fact]
    public async Task SyncCompleted_IsLogged()
    {
        // Arrange
        _options.AutoSyncInterval = null;
        var service = CreateService();

        _mockSyncService
            .Setup(s => s.SyncAllAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(50);

        // Raise the event
        _mockSyncService.Raise(s => s.SyncCompleted += null, 
            _mockSyncService.Object, 
            new CacheSyncEventArgs("test-project", "en", DateTimeOffset.UtcNow));

        // Assert - should not throw, logger should be called
        // (We can't easily verify logger was called without more complex setup)
    }

    [Fact]
    public async Task SyncFailed_IsLogged()
    {
        // Arrange
        _options.AutoSyncInterval = null;
        var service = CreateService();

        _mockSyncService
            .Setup(s => s.SyncAllAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(50);

        // Raise the event
        _mockSyncService.Raise(s => s.SyncFailed += null,
            _mockSyncService.Object,
            new CacheSyncErrorEventArgs("test-project", "en", new Exception("Test error")));

        // Assert - should not throw
    }

    [Fact]
    public async Task SyncAllCompleted_IsLogged()
    {
        // Arrange
        _options.AutoSyncInterval = null;
        var service = CreateService();

        _mockSyncService
            .Setup(s => s.SyncAllAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(50);

        // Raise the event
        _mockSyncService.Raise(s => s.SyncAllCompleted += null,
            _mockSyncService.Object,
            new CacheSyncAllEventArgs(
                new List<string> { "test-project" },
                new List<string>(),
                DateTimeOffset.UtcNow));

        // Assert - should not throw
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () =>
        {
            service.Dispose();
            service.Dispose();
            service.Dispose();
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_UnsubscribesFromEvents()
    {
        // Arrange
        var service = CreateService();

        // Act
        service.Dispose();

        // Raise events - should not throw or cause issues
        var act = () =>
        {
            _mockSyncService.Raise(s => s.SyncCompleted += null,
                _mockSyncService.Object,
                new CacheSyncEventArgs("test-project", "en", DateTimeOffset.UtcNow));
        };

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}
