using System;

using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;

using Moq;

using Translaas.Caching;

using Xunit;

namespace Translaas.Caching.Tests;

/// <summary>
/// Tests for the MemoryCacheProvider class.
/// </summary>
public class MemoryCacheProviderTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCacheIsNull()
    {
        // Arrange & Act
        Action act = () => new MemoryCacheProvider(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cache");
    }

    [Fact]
    public void Constructor_ShouldInitialize_WhenCacheIsProvided()
    {
        // Arrange
        var mockCache = new Mock<IMemoryCache>();

        // Act
        var provider = new MemoryCacheProvider(mockCache.Object);

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void TryGetValue_ShouldThrowArgumentNullException_WhenKeyIsNull()
    {
        // Arrange
        var mockCache = new Mock<IMemoryCache>();
        var provider = new MemoryCacheProvider(mockCache.Object);

        // Act
        Action act = () => provider.TryGetValue<string>(null!, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrue_WhenValueExists()
    {
        // Arrange
        var mockCache = new Mock<IMemoryCache>();
        var key = "test-key";
        var expectedValue = "test-value";
        object? cachedValue = expectedValue;

        mockCache.Setup(c => c.TryGetValue(key, out cachedValue))
            .Returns(true);

        var provider = new MemoryCacheProvider(mockCache.Object);

        // Act
        var result = provider.TryGetValue<string>(key, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(expectedValue);
        mockCache.Verify(c => c.TryGetValue(key, out It.Ref<object?>.IsAny), Times.Once);
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenValueDoesNotExist()
    {
        // Arrange
        var mockCache = new Mock<IMemoryCache>();
        var key = "test-key";
        object? cachedValue = null;

        mockCache.Setup(c => c.TryGetValue(key, out cachedValue))
            .Returns(false);

        var provider = new MemoryCacheProvider(mockCache.Object);

        // Act
        var result = provider.TryGetValue<string>(key, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
        mockCache.Verify(c => c.TryGetValue(key, out It.Ref<object?>.IsAny), Times.Once);
    }

    [Fact]
    public void TryGetValue_ShouldWorkWithDifferentTypes()
    {
        // Arrange
        var mockCache = new Mock<IMemoryCache>();
        var key = "test-key";
        var expectedValue = 42;
        object? cachedValue = expectedValue;

        mockCache.Setup(c => c.TryGetValue(key, out cachedValue))
            .Returns(true);

        var provider = new MemoryCacheProvider(mockCache.Object);

        // Act
        var result = provider.TryGetValue<int>(key, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(expectedValue);
    }

    [Fact]
    public void Set_ShouldThrowArgumentNullException_WhenKeyIsNull()
    {
        // Arrange
        var mockCache = new Mock<IMemoryCache>();
        var provider = new MemoryCacheProvider(mockCache.Object);

        // Act
        Action act = () => provider.Set<string>(null!, "value");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public void Set_ShouldCallCacheSet_WhenKeyAndValueAreProvided()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var provider = new MemoryCacheProvider(memoryCache);
        var key = "test-key";
        var value = "test-value";

        // Act
        provider.Set(key, value);

        // Assert
        var result = provider.TryGetValue<string>(key, out var cachedValue);
        result.Should().BeTrue();
        cachedValue.Should().Be(value);
    }

    [Fact]
    public void Set_ShouldSetAbsoluteExpiration_WhenProvided()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var provider = new MemoryCacheProvider(memoryCache);
        var key = "test-key";
        var value = "test-value";
        var absoluteExpiration = TimeSpan.FromMilliseconds(100);

        // Act
        provider.Set(key, value, absoluteExpiration: absoluteExpiration);

        // Assert
        var result = provider.TryGetValue<string>(key, out var cachedValue);
        result.Should().BeTrue();
        cachedValue.Should().Be(value);

        // Wait for expiration
        System.Threading.Thread.Sleep(150);
        var expiredResult = provider.TryGetValue<string>(key, out _);
        expiredResult.Should().BeFalse();
    }

    [Fact]
    public void Set_ShouldSetSlidingExpiration_WhenProvided()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var provider = new MemoryCacheProvider(memoryCache);
        var key = "test-key";
        var value = "test-value";
        var slidingExpiration = TimeSpan.FromMilliseconds(100);

        // Act
        provider.Set(key, value, slidingExpiration: slidingExpiration);

        // Assert
        var result = provider.TryGetValue<string>(key, out var cachedValue);
        result.Should().BeTrue();
        cachedValue.Should().Be(value);

        // Access again before expiration to reset sliding window
        System.Threading.Thread.Sleep(50);
        provider.TryGetValue<string>(key, out _);

        // Wait for expiration after last access
        System.Threading.Thread.Sleep(150);
        var expiredResult = provider.TryGetValue<string>(key, out _);
        expiredResult.Should().BeFalse();
    }

    [Fact]
    public void Set_ShouldSetBothExpirations_WhenBothProvided()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var provider = new MemoryCacheProvider(memoryCache);
        var key = "test-key";
        var value = "test-value";
        var absoluteExpiration = TimeSpan.FromMilliseconds(200);
        var slidingExpiration = TimeSpan.FromMilliseconds(100);

        // Act
        provider.Set(key, value, absoluteExpiration, slidingExpiration);

        // Assert
        var result = provider.TryGetValue<string>(key, out var cachedValue);
        result.Should().BeTrue();
        cachedValue.Should().Be(value);
    }

    [Fact]
    public void Set_ShouldWorkWithDifferentTypes()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var provider = new MemoryCacheProvider(memoryCache);
        var key = "test-key";
        var value = 42;

        // Act
        provider.Set(key, value);

        // Assert
        var result = provider.TryGetValue<int>(key, out var cachedValue);
        result.Should().BeTrue();
        cachedValue.Should().Be(value);
    }

    [Fact]
    public void Remove_ShouldThrowArgumentNullException_WhenKeyIsNull()
    {
        // Arrange
        var mockCache = new Mock<IMemoryCache>();
        var provider = new MemoryCacheProvider(mockCache.Object);

        // Act
        Action act = () => provider.Remove(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public void Remove_ShouldCallCacheRemove_WhenKeyIsProvided()
    {
        // Arrange
        var mockCache = new Mock<IMemoryCache>();
        var key = "test-key";

        var provider = new MemoryCacheProvider(mockCache.Object);

        // Act
        provider.Remove(key);

        // Assert
        mockCache.Verify(c => c.Remove(key), Times.Once);
    }

    [Fact]
    public void Remove_ShouldWorkWithEmptyKey()
    {
        // Arrange
        var mockCache = new Mock<IMemoryCache>();
        var key = string.Empty;

        var provider = new MemoryCacheProvider(mockCache.Object);

        // Act
        provider.Remove(key);

        // Assert
        mockCache.Verify(c => c.Remove(key), Times.Once);
    }
}
