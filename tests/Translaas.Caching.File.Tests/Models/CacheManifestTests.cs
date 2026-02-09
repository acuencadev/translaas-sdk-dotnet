using FluentAssertions;

using Translaas.Caching.File.Models;

namespace Translaas.Caching.File.Tests.Models;

public class CacheManifestTests
{
    [Fact]
    public void CurrentVersion_ShouldBe1_0()
    {
        // Assert
        CacheManifest.CurrentVersion.Should().Be("1.0");
    }

    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var manifest = new CacheManifest();

        // Assert
        manifest.Version.Should().Be(CacheManifest.CurrentVersion);
        manifest.SdkVersion.Should().BeEmpty();
        manifest.CreatedAt.Should().Be(default);
        manifest.LastSyncAt.Should().BeNull();
        manifest.Projects.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Version_ShouldBeSettable()
    {
        // Arrange
        var manifest = new CacheManifest
        {
            // Act
            Version = "2.0"
        };

        // Assert
        manifest.Version.Should().Be("2.0");
    }

    [Fact]
    public void SdkVersion_ShouldBeSettable()
    {
        // Arrange
        var manifest = new CacheManifest
        {
            // Act
            SdkVersion = "1.0.0"
        };

        // Assert
        manifest.SdkVersion.Should().Be("1.0.0");
    }

    [Fact]
    public void CreatedAt_ShouldBeSettable()
    {
        // Arrange
        var manifest = new CacheManifest();
        var createdAt = DateTimeOffset.UtcNow;

        // Act
        manifest.CreatedAt = createdAt;

        // Assert
        manifest.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void LastSyncAt_ShouldBeSettable()
    {
        // Arrange
        var manifest = new CacheManifest();
        var lastSyncAt = DateTimeOffset.UtcNow;

        // Act
        manifest.LastSyncAt = lastSyncAt;

        // Assert
        manifest.LastSyncAt.Should().Be(lastSyncAt);
    }

    [Fact]
    public void Projects_ShouldAllowAddingProjectInfo()
    {
        // Arrange
        var manifest = new CacheManifest();
        var projectInfo = new ProjectCacheInfo
        {
            Languages = ["en", "es"],
            Status = CacheSyncStatus.Synced,
            LastSyncAt = DateTimeOffset.UtcNow
        };

        // Act
        manifest.Projects["my-project"] = projectInfo;

        // Assert
        manifest.Projects.Should().ContainKey("my-project");
        manifest.Projects["my-project"].Languages.Should().BeEquivalentTo(["en", "es"]);
    }
}
