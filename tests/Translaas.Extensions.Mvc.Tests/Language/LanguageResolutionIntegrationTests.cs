using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Translaas.Extensions.DependencyInjection;

using Xunit;

namespace Translaas.Extensions.Mvc.Tests.Language;

/// <summary>
/// Integration tests for language resolution in MVC scenarios.
/// </summary>
public class LanguageResolutionIntegrationTests
{
    [Fact]
    public void AddTranslaas_WithUseRequest_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://api.test.com";
            options.DefaultLanguage = "en";
        }, language => language
            .UseRequest()
            .UseCulture()
            .UseDefault());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Should be able to resolve IHttpContextAccessor
        var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
        httpContextAccessor.Should().NotBeNull();

        // Should be able to resolve RequestLanguageOptions
        var requestOptions = serviceProvider.GetService<RequestLanguageOptions>();
        requestOptions.Should().NotBeNull();

        // Should be able to resolve ILanguageResolver
        using (var scope = serviceProvider.CreateScope())
        {
            var resolver = scope.ServiceProvider.GetService<ILanguageResolver>();
            resolver.Should().NotBeNull();
        }

        // Should be able to resolve ITranslaasService
        using (var scope = serviceProvider.CreateScope())
        {
            var service = scope.ServiceProvider.GetService<ITranslaasService>();
            service.Should().NotBeNull();
        }
    }

    [Fact]
    public void UseRequest_WithCustomConfiguration_AppliesSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://api.test.com";
        }, language => language.UseRequest(request =>
        {
            request.RouteParameterName = "culture";
            request.QueryParameterName = "locale";
            request.HeaderName = "X-Custom-Lang";
            request.CookieName = "language";
            request.Sources = new System.Collections.Generic.List<RequestLanguageSource>
            {
                RequestLanguageSource.QueryString,
                RequestLanguageSource.Cookie
            };
        }));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<RequestLanguageOptions>();
        
        options.RouteParameterName.Should().Be("culture");
        options.QueryParameterName.Should().Be("locale");
        options.HeaderName.Should().Be("X-Custom-Lang");
        options.CookieName.Should().Be("language");
        options.Sources.Should().HaveCount(2);
        options.Sources[0].Should().Be(RequestLanguageSource.QueryString);
        options.Sources[1].Should().Be(RequestLanguageSource.Cookie);
    }

    [Fact]
    public void RequestLanguageProvider_ResolvesLanguage_FromHttpContext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://api.test.com";
        }, language => language.UseRequest(request =>
        {
            request.Sources = new System.Collections.Generic.List<RequestLanguageSource>
            {
                RequestLanguageSource.QueryString
            };
        }));

        var serviceProvider = services.BuildServiceProvider();

        // Create HTTP context with query parameter
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Query = new Microsoft.AspNetCore.Http.QueryCollection(
            new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "lang", "fr" }
            });
        
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;

        // Act
        using (var scope = serviceProvider.CreateScope())
        {
            var resolver = scope.ServiceProvider.GetRequiredService<ILanguageResolver>();
            var result = resolver.Resolve();
        }

        // Assert - resolver should be able to resolve language from HTTP context
        using (var scope = serviceProvider.CreateScope())
        {
            var resolver = scope.ServiceProvider.GetRequiredService<ILanguageResolver>();
            var result = resolver.Resolve();
            result.Should().Be("fr");
        }
    }

    [Fact]
    public void UseRequest_CanBeCombined_WithOtherProviders()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://api.test.com";
            options.DefaultLanguage = "en";
        }, language => language
            .UseRequest()      // First: check HTTP request
            .UseCulture()       // Second: check thread culture
            .UseDefault());     // Third: use configured default

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        using (var scope = serviceProvider.CreateScope())
        {
            var providers = scope.ServiceProvider.GetServices<ILanguageProvider>().ToList();
            providers.Should().HaveCount(3);
            providers[0].Should().BeOfType<RequestLanguageProvider>();
            providers[1].Should().BeOfType<CultureLanguageProvider>();
            providers[2].Should().BeOfType<DefaultLanguageProvider>();
        }
    }
}
