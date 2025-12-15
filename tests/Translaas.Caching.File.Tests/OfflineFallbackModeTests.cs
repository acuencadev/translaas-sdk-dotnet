using FluentAssertions;

using Translaas.Caching.File;

namespace Translaas.Caching.File.Tests;

public class OfflineFallbackModeTests
{
    [Fact]
    public void CacheFirst_ShouldBeDefaultValue()
    {
        // Arrange & Act
        var defaultValue = default(OfflineFallbackMode);

        // Assert
        defaultValue.Should().Be(OfflineFallbackMode.CacheFirst);
    }

    [Fact]
    public void AllModes_ShouldHaveExpectedValues()
    {
        // Assert
        ((int)OfflineFallbackMode.CacheFirst).Should().Be(0);
        ((int)OfflineFallbackMode.ApiFirst).Should().Be(1);
        ((int)OfflineFallbackMode.CacheOnly).Should().Be(2);
        ((int)OfflineFallbackMode.ApiOnlyWithBackup).Should().Be(3);
    }

    [Fact]
    public void Enum_ShouldHaveFourValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<OfflineFallbackMode>();

        // Assert
        values.Should().HaveCount(4);
    }
}
