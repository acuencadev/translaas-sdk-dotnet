using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Translaas.Caching;

namespace Translaas.Extensions.DependencyInjection.Tests;

/// <summary>
/// Tests for the AddTranslaas overload with IConfiguration.
/// </summary>
public class AddTranslaasConfigurationTests
{
    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindFromTranslaasSection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "config-api-key" },
                { "Translaas:BaseUrl", "https://api.config.com" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.ApiKey.Should().Be("config-api-key");
        options.Value.BaseUrl.Should().Be("https://api.config.com");
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindCacheMode()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:CacheMode", "Group" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.CacheMode.Should().Be(CacheMode.Group);
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindTimeout()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:Timeout", "00:00:30" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.Timeout.Should().Be(System.TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindCacheExpirationSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:CacheAbsoluteExpiration", "01:00:00" },
                { "Translaas:CacheSlidingExpiration", "00:15:00" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.CacheAbsoluteExpiration.Should().Be(System.TimeSpan.FromHours(1));
        options.Value.CacheSlidingExpiration.Should().Be(System.TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldUseCustomSectionName()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "CustomSection:ApiKey", "custom-key" },
                { "CustomSection:BaseUrl", "https://custom.api.com" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration, "CustomSection");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.ApiKey.Should().Be("custom-key");
        options.Value.BaseUrl.Should().Be("https://custom.api.com");
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() =>
            services.AddTranslaas((IConfiguration)null!));
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldThrowArgumentNullException_WhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() =>
            services!.AddTranslaas(configuration));
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldRegisterITranslaasClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetService<Translaas.Client.ITranslaasClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldReturnIServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" }
            })
            .Build();

        // Act
        var result = services.AddTranslaas(configuration);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldThrowInvalidOperationException_WhenApiKeyIsMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:BaseUrl", "https://api.test.com" }
                // ApiKey is missing
            })
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddTranslaas(configuration));
        
        exception.Message.Should().Contain("ApiKey is required");
        exception.Message.Should().Contain("Translaas:ApiKey");
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldThrowInvalidOperationException_WhenBaseUrlIsMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" }
                // BaseUrl is missing
            })
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddTranslaas(configuration));
        
        exception.Message.Should().Contain("BaseUrl is required");
        exception.Message.Should().Contain("Translaas:BaseUrl");
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldThrowArgumentException_WhenSectionNameIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" }
            })
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.AddTranslaas(configuration, string.Empty));
    }
}

