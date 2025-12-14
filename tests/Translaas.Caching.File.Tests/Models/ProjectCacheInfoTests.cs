using FluentAssertions;

using Translaas.Caching.File.Models;

namespace Translaas.Caching.File.Tests.Models;

public class ProjectCacheInfoTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var info = new ProjectCacheInfo();

        // Assert
        info.Languages.Should().NotBeNull().And.BeEmpty();
        info.LastSyncAt.Should().BeNull();
        info.Status.Should().Be(CacheSyncStatus.Pending);
    }

    [Fact]
    public void Languages_ShouldBeSettable()
    {
        // Arrange
        var info = new ProjectCacheInfo();
        var languages = new List<string> { "en", "es", "fr" };

        // Act
        info.Languages = languages;

        // Assert
        info.Languages.Should().BeEquivalentTo(languages);
    }

    [Fact]
    public void Languages_ShouldAllowAdding()
    {
        // Arrange
        var info = new ProjectCacheInfo();

        // Act
        info.Languages.Add("en");
        info.Languages.Add("es");

        // Assert
        info.Languages.Should().HaveCount(2);
        info.Languages.Should().Contain("en");
        info.Languages.Should().Contain("es");
    }

    [Fact]
    public void LastSyncAt_ShouldBeSettable()
    {
        // Arrange
        var info = new ProjectCacheInfo();
        var syncTime = DateTimeOffset.UtcNow;

        // Act
        info.LastSyncAt = syncTime;

        // Assert
        info.LastSyncAt.Should().Be(syncTime);
    }

    [Fact]
    public void Status_ShouldBeSettable()
    {
        // Arrange
        var info = new ProjectCacheInfo();

        // Act
        info.Status = CacheSyncStatus.Synced;

        // Assert
        info.Status.Should().Be(CacheSyncStatus.Synced);
    }

    [Theory]
    [InlineData(CacheSyncStatus.Pending)]
    [InlineData(CacheSyncStatus.Syncing)]
    [InlineData(CacheSyncStatus.Synced)]
    [InlineData(CacheSyncStatus.Failed)]
    [InlineData(CacheSyncStatus.Stale)]
    public void Status_ShouldSupportAllValues(CacheSyncStatus status)
    {
        // Arrange
        var info = new ProjectCacheInfo();

        // Act
        info.Status = status;

        // Assert
        info.Status.Should().Be(status);
    }
}
