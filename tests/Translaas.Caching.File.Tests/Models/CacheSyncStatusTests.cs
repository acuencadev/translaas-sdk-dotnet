using FluentAssertions;

using Translaas.Caching.File.Models;

namespace Translaas.Caching.File.Tests.Models;

public class CacheSyncStatusTests
{
    [Fact]
    public void Pending_ShouldBeDefaultValue()
    {
        // Arrange & Act
        var defaultValue = default(CacheSyncStatus);

        // Assert
        defaultValue.Should().Be(CacheSyncStatus.Pending);
    }

    [Fact]
    public void AllStatuses_ShouldHaveExpectedValues()
    {
        // Assert
        ((int)CacheSyncStatus.Pending).Should().Be(0);
        ((int)CacheSyncStatus.Syncing).Should().Be(1);
        ((int)CacheSyncStatus.Synced).Should().Be(2);
        ((int)CacheSyncStatus.Failed).Should().Be(3);
        ((int)CacheSyncStatus.Stale).Should().Be(4);
    }

    [Fact]
    public void Enum_ShouldHaveFiveValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<CacheSyncStatus>();

        // Assert
        values.Should().HaveCount(5);
    }
}
