using FluentAssertions;

using Translaas.Caching;

namespace Translaas.Extensions.DependencyInjection.Tests;

/// <summary>
/// Tests for the TranslaasOptions class.
/// </summary>
public class TranslaasOptionsTests
{
    [Fact]
    public void TranslaasOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new TranslaasOptions();

        // Assert
        options.ApiKey.Should().BeEmpty();
        options.BaseUrl.Should().NotBeNullOrEmpty();
        options.CacheMode.Should().Be(CacheMode.None);
        options.Timeout.Should().BeNull();
        options.CacheAbsoluteExpiration.Should().BeNull();
        options.CacheSlidingExpiration.Should().BeNull();
    }

    [Fact]
    public void TranslaasOptions_ShouldAllowSettingApiKey()
    {
        // Arrange
        var options = new TranslaasOptions();
        var apiKey = "test-api-key-123";

        // Act
        options.ApiKey = apiKey;

        // Assert
        options.ApiKey.Should().Be(apiKey);
    }

    [Fact]
    public void TranslaasOptions_ShouldAllowSettingBaseUrl()
    {
        // Arrange
        var options = new TranslaasOptions();
        var baseUrl = "https://api.test.com";

        // Act
        options.BaseUrl = baseUrl;

        // Assert
        options.BaseUrl.Should().Be(baseUrl);
    }

    [Fact]
    public void TranslaasOptions_ShouldAllowSettingCacheMode()
    {
        // Arrange
        var options = new TranslaasOptions
        {
            // Act & Assert
            CacheMode = CacheMode.Entry
        };
        options.CacheMode.Should().Be(CacheMode.Entry);

        options.CacheMode = CacheMode.Group;
        options.CacheMode.Should().Be(CacheMode.Group);

        options.CacheMode = CacheMode.Project;
        options.CacheMode.Should().Be(CacheMode.Project);

        options.CacheMode = CacheMode.None;
        options.CacheMode.Should().Be(CacheMode.None);
    }

    [Fact]
    public void TranslaasOptions_ShouldAllowSettingTimeout()
    {
        // Arrange
        var options = new TranslaasOptions();
        var timeout = TimeSpan.FromSeconds(30);

        // Act
        options.Timeout = timeout;

        // Assert
        options.Timeout.Should().Be(timeout);
    }

    [Fact]
    public void TranslaasOptions_ShouldAllowSettingCacheAbsoluteExpiration()
    {
        // Arrange
        var options = new TranslaasOptions();
        var expiration = TimeSpan.FromHours(1);

        // Act
        options.CacheAbsoluteExpiration = expiration;

        // Assert
        options.CacheAbsoluteExpiration.Should().Be(expiration);
    }

    [Fact]
    public void TranslaasOptions_ShouldAllowSettingCacheSlidingExpiration()
    {
        // Arrange
        var options = new TranslaasOptions();
        var expiration = TimeSpan.FromMinutes(15);

        // Act
        options.CacheSlidingExpiration = expiration;

        // Assert
        options.CacheSlidingExpiration.Should().Be(expiration);
    }

    [Fact]
    public void TranslaasOptions_ShouldAllowNullTimeout()
    {
        // Arrange
        var options = new TranslaasOptions
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Act
        options.Timeout = null;

        // Assert
        options.Timeout.Should().BeNull();
    }

    [Fact]
    public void TranslaasOptions_ShouldAllowNullCacheAbsoluteExpiration()
    {
        // Arrange
        var options = new TranslaasOptions
        {
            CacheAbsoluteExpiration = TimeSpan.FromHours(1)
        };

        // Act
        options.CacheAbsoluteExpiration = null;

        // Assert
        options.CacheAbsoluteExpiration.Should().BeNull();
    }

    [Fact]
    public void TranslaasOptions_ShouldAllowNullCacheSlidingExpiration()
    {
        // Arrange
        var options = new TranslaasOptions
        {
            CacheSlidingExpiration = TimeSpan.FromMinutes(15)
        };

        // Act
        options.CacheSlidingExpiration = null;

        // Assert
        options.CacheSlidingExpiration.Should().BeNull();
    }
}
