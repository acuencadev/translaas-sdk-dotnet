using FluentAssertions;

using Translaas.Caching;

using Xunit;

namespace Translaas.Caching.Tests;

/// <summary>
/// Tests for the CacheMode enum.
/// </summary>
public class CacheModeTests
{
    [Fact]
    public void CacheMode_ShouldHaveNoneValue()
    {
        // Arrange & Act
        var mode = CacheMode.None;

        // Assert
        mode.Should().Be(CacheMode.None);
        ((int)mode).Should().Be(0);
    }

    [Fact]
    public void CacheMode_ShouldHaveEntryValue()
    {
        // Arrange & Act
        var mode = CacheMode.Entry;

        // Assert
        mode.Should().Be(CacheMode.Entry);
        ((int)mode).Should().Be(1);
    }

    [Fact]
    public void CacheMode_ShouldHaveGroupValue()
    {
        // Arrange & Act
        var mode = CacheMode.Group;

        // Assert
        mode.Should().Be(CacheMode.Group);
        ((int)mode).Should().Be(2);
    }

    [Fact]
    public void CacheMode_ShouldHaveProjectValue()
    {
        // Arrange & Act
        var mode = CacheMode.Project;

        // Assert
        mode.Should().Be(CacheMode.Project);
        ((int)mode).Should().Be(3);
    }

    [Theory]
    [InlineData(0, CacheMode.None)]
    [InlineData(1, CacheMode.Entry)]
    [InlineData(2, CacheMode.Group)]
    [InlineData(3, CacheMode.Project)]
    public void CacheMode_ShouldCastFromInt(int value, CacheMode expectedMode)
    {
        // Act
        var mode = (CacheMode)value;

        // Assert
        mode.Should().Be(expectedMode);
    }

    [Fact]
    public void CacheMode_ShouldHaveAllExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<CacheMode>();

        // Assert
        values.Should().HaveCount(4);
        values.Should().Contain(CacheMode.None);
        values.Should().Contain(CacheMode.Entry);
        values.Should().Contain(CacheMode.Group);
        values.Should().Contain(CacheMode.Project);
    }
}
