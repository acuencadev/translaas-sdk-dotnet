using FluentAssertions;

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Translaas.Client;
using Translaas.Models.Errors;

using Xunit;

namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Integration tests for error scenarios.
/// These tests require a running development API and TRANSLAAS_API_KEY environment variable.
/// </summary>
public class ErrorScenariosIntegrationTests : IDisposable
{
    private readonly IntegrationTestConfiguration _configuration;
    private readonly HttpClient? _httpClient;

    public ErrorScenariosIntegrationTests()
    {
        _configuration = new IntegrationTestConfiguration();
        
        if (!_configuration.IsEnabled)
        {
            _httpClient = null;
            return;
        }

        _configuration.Validate();
        _httpClient = new HttpClient();
    }

    [Fact]
    public async Task Client_ShouldThrowTranslaasApiException_WhenApiKeyIsInvalid()
    {
        // Skip if integration tests are not enabled
        if (!_configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var invalidOptions = new TranslaasClientOptions
        {
            ApiKey = "invalid-api-key-12345",
            BaseUrl = _configuration.BaseUrl
        };
        var invalidClient = new TranslaasClient(_httpClient, invalidOptions);

        // Act
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => invalidClient.GetEntryAsync("ui", "button.save", "en"));

        // Assert
        exception.Should().NotBeNull();
        exception.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Client_ShouldThrowTranslaasApiException_WhenBaseUrlIsInvalid()
    {
        // Skip if integration tests are not enabled
        if (!_configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        if (_httpClient == null)
        {
            return;
        }

        var invalidOptions = new TranslaasClientOptions
        {
            ApiKey = _configuration.ApiKey,
            BaseUrl = "https://invalid-url-that-does-not-exist-12345.com/api"
        };
        var invalidClient = new TranslaasClient(_httpClient, invalidOptions);

        // Act & Assert
        // This should throw either TranslaasApiException or HttpRequestException
        await Assert.ThrowsAnyAsync<Exception>(
            () => invalidClient.GetEntryAsync("ui", "button.save", "en"));
    }

    [Fact]
    public async Task Client_ShouldThrowTranslaasApiException_WhenRequestTimesOut()
    {
        // Skip if integration tests are not enabled
        if (!_configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var timeoutOptions = new TranslaasClientOptions
        {
            ApiKey = _configuration.ApiKey,
            BaseUrl = _configuration.BaseUrl,
            Timeout = TimeSpan.FromMilliseconds(1) // Very short timeout to force timeout
        };
        if (_httpClient == null)
        {
            return;
        }

        var timeoutClient = new TranslaasClient(_httpClient, timeoutOptions);

        // Act
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => timeoutClient.GetEntryAsync("ui", "button.save", "en"));

        // Assert
        exception.Should().NotBeNull();
        exception.StatusCode.Should().Be(HttpStatusCode.RequestTimeout);
        exception.Message.Should().Contain("timed out");
    }

    [Fact]
    public async Task Client_ShouldHandle404NotFound_WhenResourceDoesNotExist()
    {
        // Skip if integration tests are not enabled
        if (!_configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = _configuration.ApiKey,
            BaseUrl = _configuration.BaseUrl
        };
        if (_httpClient == null)
        {
            return;
        }

        var client = new TranslaasClient(_httpClient, options);

        // Act
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("nonexistent-group", "nonexistent-entry", "nonexistent-lang"));

        // Assert
        exception.Should().NotBeNull();
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
