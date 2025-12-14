using System;
using System.Collections.Generic;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Translaas.Caching.File;
using Translaas.Extensions.DependencyInjection;

using Xunit;

namespace Translaas.Extensions.DependencyInjection.Tests;

/// <summary>
/// Tests for configuration binding of OfflineCacheOptions from appsettings.json.
/// </summary>
public class OfflineCacheConfigurationBindingTests
{
    #region OfflineCacheOptions Binding Tests

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindOfflineCacheEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:Projects:0", "my-project" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.Enabled.Should().BeTrue();
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindOfflineCacheDirectory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:CacheDirectory", "./custom-cache" },
                { "Translaas:OfflineCache:Projects:0", "my-project" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.CacheDirectory.Should().Be("./custom-cache");
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindOfflineFallbackMode()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:FallbackMode", "ApiFirst" },
                { "Translaas:OfflineCache:Projects:0", "my-project" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.FallbackMode.Should().Be(OfflineFallbackMode.ApiFirst);
    }

    [Theory]
    [InlineData("CacheFirst", OfflineFallbackMode.CacheFirst)]
    [InlineData("ApiFirst", OfflineFallbackMode.ApiFirst)]
    [InlineData("CacheOnly", OfflineFallbackMode.CacheOnly)]
    [InlineData("ApiOnlyWithBackup", OfflineFallbackMode.ApiOnlyWithBackup)]
    public void AddTranslaas_WithConfiguration_ShouldBindAllFallbackModes(string configValue, OfflineFallbackMode expected)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:FallbackMode", configValue },
                { "Translaas:OfflineCache:Projects:0", "my-project" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.FallbackMode.Should().Be(expected);
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindAutoSyncSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:AutoSync", "false" },
                { "Translaas:OfflineCache:AutoSyncInterval", "02:00:00" },
                { "Translaas:OfflineCache:Projects:0", "my-project" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.AutoSync.Should().BeFalse();
        options.Value.OfflineCache.AutoSyncInterval.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindProjectsList()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:Projects:0", "project-one" },
                { "Translaas:OfflineCache:Projects:1", "project-two" },
                { "Translaas:OfflineCache:Projects:2", "project-three" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.Projects.Should().HaveCount(3);
        options.Value.OfflineCache.Projects.Should().BeEquivalentTo(new[] { "project-one", "project-two", "project-three" });
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindLanguagesList()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:Projects:0", "my-project" },
                { "Translaas:OfflineCache:Languages:0", "en" },
                { "Translaas:OfflineCache:Languages:1", "es" },
                { "Translaas:OfflineCache:Languages:2", "fr" },
                { "Translaas:OfflineCache:Languages:3", "de" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.Languages.Should().HaveCount(4);
        options.Value.OfflineCache.Languages.Should().BeEquivalentTo(new[] { "en", "es", "fr", "de" });
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindDefaultProjectId()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:DefaultProjectId", "explicit-default-project" },
                { "Translaas:OfflineCache:Projects:0", "other-project" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.DefaultProjectId.Should().Be("explicit-default-project");
    }

    #endregion

    #region HybridCacheOptions Binding Tests

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindHybridCacheEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:Projects:0", "my-project" },
                { "Translaas:OfflineCache:HybridCache:Enabled", "false" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.HybridCache.Enabled.Should().BeFalse();
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindHybridCacheExpiration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:Projects:0", "my-project" },
                { "Translaas:OfflineCache:HybridCache:MemoryCacheExpiration", "01:30:00" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.HybridCache.MemoryCacheExpiration.Should().Be(TimeSpan.FromMinutes(90));
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindMaxMemoryCacheEntries()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:Projects:0", "my-project" },
                { "Translaas:OfflineCache:HybridCache:MaxMemoryCacheEntries", "5000" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.HybridCache.MaxMemoryCacheEntries.Should().Be(5000);
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindWarmupOnStartup()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:Projects:0", "my-project" },
                { "Translaas:OfflineCache:HybridCache:WarmupOnStartup", "true" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        options.Value.OfflineCache.HybridCache.WarmupOnStartup.Should().BeTrue();
    }

    #endregion

    #region Complete Configuration Tests

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindCompleteOfflineCacheConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:CacheDirectory", "./my-translations" },
                { "Translaas:OfflineCache:FallbackMode", "CacheFirst" },
                { "Translaas:OfflineCache:AutoSync", "true" },
                { "Translaas:OfflineCache:AutoSyncInterval", "04:00:00" },
                { "Translaas:OfflineCache:DefaultProjectId", "main-project" },
                { "Translaas:OfflineCache:Projects:0", "main-project" },
                { "Translaas:OfflineCache:Projects:1", "secondary-project" },
                { "Translaas:OfflineCache:Languages:0", "en" },
                { "Translaas:OfflineCache:Languages:1", "es" },
                { "Translaas:OfflineCache:HybridCache:Enabled", "true" },
                { "Translaas:OfflineCache:HybridCache:MemoryCacheExpiration", "00:45:00" },
                { "Translaas:OfflineCache:HybridCache:MaxMemoryCacheEntries", "2000" },
                { "Translaas:OfflineCache:HybridCache:WarmupOnStartup", "true" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();

        // Verify OfflineCacheOptions
        options.Value.OfflineCache.Enabled.Should().BeTrue();
        options.Value.OfflineCache.CacheDirectory.Should().Be("./my-translations");
        options.Value.OfflineCache.FallbackMode.Should().Be(OfflineFallbackMode.CacheFirst);
        options.Value.OfflineCache.AutoSync.Should().BeTrue();
        options.Value.OfflineCache.AutoSyncInterval.Should().Be(TimeSpan.FromHours(4));
        options.Value.OfflineCache.DefaultProjectId.Should().Be("main-project");
        options.Value.OfflineCache.Projects.Should().BeEquivalentTo(new[] { "main-project", "secondary-project" });
        options.Value.OfflineCache.Languages.Should().BeEquivalentTo(new[] { "en", "es" });

        // Verify HybridCacheOptions
        options.Value.OfflineCache.HybridCache.Enabled.Should().BeTrue();
        options.Value.OfflineCache.HybridCache.MemoryCacheExpiration.Should().Be(TimeSpan.FromMinutes(45));
        options.Value.OfflineCache.HybridCache.MaxMemoryCacheEntries.Should().Be(2000);
        options.Value.OfflineCache.HybridCache.WarmupOnStartup.Should().BeTrue();
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldUseDefaultValues_WhenNotSpecified()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" }
                // No OfflineCache configuration
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();

        // Verify defaults
        options.Value.OfflineCache.Enabled.Should().BeFalse();
        options.Value.OfflineCache.CacheDirectory.Should().Be(OfflineCacheOptions.DefaultCacheDirectory);
        options.Value.OfflineCache.FallbackMode.Should().Be(OfflineFallbackMode.CacheFirst);
        options.Value.OfflineCache.AutoSync.Should().BeTrue();
        options.Value.OfflineCache.AutoSyncInterval.Should().Be(TimeSpan.FromHours(1));
        options.Value.OfflineCache.Projects.Should().BeEmpty();
        options.Value.OfflineCache.Languages.Should().BeEmpty();

        options.Value.OfflineCache.HybridCache.Enabled.Should().BeTrue();
        options.Value.OfflineCache.HybridCache.MemoryCacheExpiration.Should().Be(TimeSpan.FromMinutes(30));
        options.Value.OfflineCache.HybridCache.MaxMemoryCacheEntries.Should().Be(1000);
        options.Value.OfflineCache.HybridCache.WarmupOnStartup.Should().BeFalse();
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldRegisterOfflineCacheServices_WhenEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:Projects:0", "my-project" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // IOfflineCacheProvider should be registered
        var cacheProvider = serviceProvider.GetService<IOfflineCacheProvider>();
        cacheProvider.Should().NotBeNull();

        // OfflineCacheOptions should be registered
        var offlineOptions = serviceProvider.GetService<OfflineCacheOptions>();
        offlineOptions.Should().NotBeNull();
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldRegisterHybridCacheProvider_WhenHybridEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:Projects:0", "my-project" },
                { "Translaas:OfflineCache:HybridCache:Enabled", "true" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var cacheProvider = serviceProvider.GetService<IOfflineCacheProvider>();
        cacheProvider.Should().NotBeNull();
        cacheProvider.Should().BeOfType<HybridCacheProvider>();
    }

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldRegisterFileCacheProvider_WhenHybridDisabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "test-key" },
                { "Translaas:BaseUrl", "https://api.test.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:Projects:0", "my-project" },
                { "Translaas:OfflineCache:HybridCache:Enabled", "false" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var cacheProvider = serviceProvider.GetService<IOfflineCacheProvider>();
        cacheProvider.Should().NotBeNull();
        cacheProvider.Should().BeOfType<FileCacheProvider>();
    }

    #endregion

    #region JSON-like Configuration Tests

    [Fact]
    public void AddTranslaas_WithConfiguration_ShouldBindFromJsonLikeStructure()
    {
        // This test simulates what appsettings.json would look like:
        // {
        //   "Translaas": {
        //     "ApiKey": "my-api-key",
        //     "BaseUrl": "https://api.translaas.com",
        //     "OfflineCache": {
        //       "Enabled": true,
        //       "CacheDirectory": "./translations-cache",
        //       "FallbackMode": "CacheFirst",
        //       "AutoSync": true,
        //       "AutoSyncInterval": "02:00:00",
        //       "Projects": ["project-a", "project-b"],
        //       "Languages": ["en", "es", "fr", "de"]
        //     }
        //   }
        // }

        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Translaas:ApiKey", "my-api-key" },
                { "Translaas:BaseUrl", "https://api.translaas.com" },
                { "Translaas:OfflineCache:Enabled", "true" },
                { "Translaas:OfflineCache:CacheDirectory", "./translations-cache" },
                { "Translaas:OfflineCache:FallbackMode", "CacheFirst" },
                { "Translaas:OfflineCache:AutoSync", "true" },
                { "Translaas:OfflineCache:AutoSyncInterval", "02:00:00" },
                { "Translaas:OfflineCache:Projects:0", "project-a" },
                { "Translaas:OfflineCache:Projects:1", "project-b" },
                { "Translaas:OfflineCache:Languages:0", "en" },
                { "Translaas:OfflineCache:Languages:1", "es" },
                { "Translaas:OfflineCache:Languages:2", "fr" },
                { "Translaas:OfflineCache:Languages:3", "de" }
            })
            .Build();

        // Act
        services.AddTranslaas(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();

        options.Value.ApiKey.Should().Be("my-api-key");
        options.Value.BaseUrl.Should().Be("https://api.translaas.com");
        options.Value.OfflineCache.Enabled.Should().BeTrue();
        options.Value.OfflineCache.CacheDirectory.Should().Be("./translations-cache");
        options.Value.OfflineCache.FallbackMode.Should().Be(OfflineFallbackMode.CacheFirst);
        options.Value.OfflineCache.AutoSync.Should().BeTrue();
        options.Value.OfflineCache.AutoSyncInterval.Should().Be(TimeSpan.FromHours(2));
        options.Value.OfflineCache.Projects.Should().BeEquivalentTo(new[] { "project-a", "project-b" });
        options.Value.OfflineCache.Languages.Should().BeEquivalentTo(new[] { "en", "es", "fr", "de" });
    }

    #endregion
}
