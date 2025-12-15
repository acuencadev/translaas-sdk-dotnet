using FluentAssertions;

using Translaas.Caching.File;

namespace Translaas.Caching.File.Tests;

public class OfflineCacheOptionsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var options = new OfflineCacheOptions();

        // Assert
        options.Enabled.Should().BeFalse();
        options.CacheDirectory.Should().Be(OfflineCacheOptions.DefaultCacheDirectory);
        options.FallbackMode.Should().Be(OfflineFallbackMode.CacheFirst);
        options.AutoSync.Should().BeTrue();
        options.AutoSyncInterval.Should().Be(TimeSpan.FromHours(1));
        options.Projects.Should().NotBeNull().And.BeEmpty();
        options.Languages.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DefaultCacheDirectory_ShouldBeTranslaasCache()
    {
        // Assert
        OfflineCacheOptions.DefaultCacheDirectory.Should().Be(".translaas-cache");
    }

    [Fact]
    public void Enabled_ShouldBeSettable()
    {
        // Arrange
        var options = new OfflineCacheOptions();

        // Act
        options.Enabled = true;

        // Assert
        options.Enabled.Should().BeTrue();
    }

    [Fact]
    public void CacheDirectory_ShouldBeSettable()
    {
        // Arrange
        var options = new OfflineCacheOptions();
        const string customDirectory = "./custom-cache";

        // Act
        options.CacheDirectory = customDirectory;

        // Assert
        options.CacheDirectory.Should().Be(customDirectory);
    }

    [Fact]
    public void FallbackMode_ShouldBeSettable()
    {
        // Arrange
        var options = new OfflineCacheOptions();

        // Act
        options.FallbackMode = OfflineFallbackMode.CacheOnly;

        // Assert
        options.FallbackMode.Should().Be(OfflineFallbackMode.CacheOnly);
    }

    [Fact]
    public void AutoSync_ShouldBeSettable()
    {
        // Arrange
        var options = new OfflineCacheOptions();

        // Act
        options.AutoSync = false;

        // Assert
        options.AutoSync.Should().BeFalse();
    }

    [Fact]
    public void AutoSyncInterval_ShouldBeSettable()
    {
        // Arrange
        var options = new OfflineCacheOptions();
        var customInterval = TimeSpan.FromMinutes(30);

        // Act
        options.AutoSyncInterval = customInterval;

        // Assert
        options.AutoSyncInterval.Should().Be(customInterval);
    }

    [Fact]
    public void AutoSyncInterval_ShouldBeSettableToNull()
    {
        // Arrange
        var options = new OfflineCacheOptions();

        // Act
        options.AutoSyncInterval = null;

        // Assert
        options.AutoSyncInterval.Should().BeNull();
    }

    [Fact]
    public void Projects_ShouldBeSettable()
    {
        // Arrange
        var options = new OfflineCacheOptions();
        var projects = new List<string> { "project1", "project2" };

        // Act
        options.Projects = projects;

        // Assert
        options.Projects.Should().BeEquivalentTo(projects);
    }

    [Fact]
    public void Projects_ShouldAllowAdding()
    {
        // Arrange
        var options = new OfflineCacheOptions();

        // Act
        options.Projects.Add("my-project");
        options.Projects.Add("another-project");

        // Assert
        options.Projects.Should().HaveCount(2);
        options.Projects.Should().Contain("my-project");
        options.Projects.Should().Contain("another-project");
    }

    [Fact]
    public void Languages_ShouldBeSettable()
    {
        // Arrange
        var options = new OfflineCacheOptions();
        var languages = new List<string> { "en", "es", "fr" };

        // Act
        options.Languages = languages;

        // Assert
        options.Languages.Should().BeEquivalentTo(languages);
    }

    [Fact]
    public void Languages_ShouldAllowAdding()
    {
        // Arrange
        var options = new OfflineCacheOptions();

        // Act
        options.Languages.Add("en");
        options.Languages.Add("es");
        options.Languages.Add("fr");

        // Assert
        options.Languages.Should().HaveCount(3);
        options.Languages.Should().Contain("en");
        options.Languages.Should().Contain("es");
        options.Languages.Should().Contain("fr");
    }
}
