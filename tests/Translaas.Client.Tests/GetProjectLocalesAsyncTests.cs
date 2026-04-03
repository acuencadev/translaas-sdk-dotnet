using System.Net;
using System.Text;

using FluentAssertions;

using Moq;
using Moq.Protected;

using Translaas.Models.Errors;

namespace Translaas.Client.Tests;

public class GetProjectLocalesAsyncTests
{
    private readonly TranslaasClientOptions _defaultOptions;

    public GetProjectLocalesAsyncTests()
    {
        _defaultOptions = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
        };
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldReturnProjectLocales_WhenRequestSucceeds()
    {
        // Arrange
        var jsonResponse = "{\"locales\":[\"en\",\"fr\",\"es\",\"de\"]}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetProjectLocalesAsync("my-project");

        // Assert
        result.Should().NotBeNull();
        result.Locales.Should().HaveCount(4);
        result.Locales.Should().Contain("en");
        result.Locales.Should().Contain("fr");
        result.Locales.Should().Contain("es");
        result.Locales.Should().Contain("de");
        VerifyHttpRequest(handlerMock, "/sdk/v1/translations/locales");
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldThrowTranslaasApiException_WhenApiReturns404()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, "Not Found", "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetProjectLocalesAsync("nonexistent-project");

        // Assert
        await act.Should().ThrowAsync<TranslaasApiException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldThrowTranslaasApiException_WhenApiReturns500()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError, "Internal Server Error", "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetProjectLocalesAsync("my-project");

        // Assert
        await act.Should().ThrowAsync<TranslaasApiException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldThrowArgumentNullException_WhenProjectIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetProjectLocalesAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "project");
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldPassCancellationToken_ToHttpClient()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var jsonResponse = "{\"locales\":[\"en\",\"fr\"]}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetProjectLocalesAsync("my-project", cancellationToken: cancellationToken);

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldSetApiKeyHeader()
    {
        // Arrange
        var jsonResponse = "{\"locales\":[\"en\",\"fr\"]}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetProjectLocalesAsync("my-project");

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.Contains("X-Api-Key") &&
                    req.Headers.GetValues("X-Api-Key").First() == "test-api-key"),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldUseGetMethod()
    {
        // Arrange
        var jsonResponse = "{\"locales\":[\"en\",\"fr\"]}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetProjectLocalesAsync("my-project");

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldUseQueryStringParameters()
    {
        // Arrange
        var jsonResponse = "{\"locales\":[\"en\",\"fr\"]}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetProjectLocalesAsync("my-project");

        // Assert - Verify query string parameters are used (not JSON body)
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.Query.Contains("project=my-project") &&
                    req.Content == null), // GET requests with query strings don't have content
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldDeserializeEmptyLocales_WhenResponseIsEmpty()
    {
        // Arrange
        var jsonResponse = "{\"locales\":[]}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetProjectLocalesAsync("my-project");

        // Assert
        result.Should().NotBeNull();
        result.Locales.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldIncludeProjectInRequest()
    {
        // Arrange
        var jsonResponse = "{\"locales\":[\"en\",\"fr\"]}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetProjectLocalesAsync("my-project");

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.Query.Contains("project=my-project")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldThrowTranslaasApiException_WhenDeserializationFails()
    {
        // Arrange
        var invalidJson = "{invalid json}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, invalidJson, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetProjectLocalesAsync("my-project");

        // Assert
        await act.Should().ThrowAsync<TranslaasApiException>()
            .Where(ex => ex.InnerException is System.Text.Json.JsonException);
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldReturnEmptyLocales_WhenApiReturns204NoContent()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NoContent, string.Empty, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetProjectLocalesAsync("nonexistent-project");

        // Assert
        result.Should().NotBeNull();
        result.Locales.Should().BeEmpty(); // Client returns empty locales when 204 No Content
        VerifyHttpRequest(handlerMock, "/sdk/v1/translations/locales");
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldThrowArgumentNullException_WhenProjectIsEmptyString()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetProjectLocalesAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "project");
    }

    private Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, string responseContent, string contentType = "application/json")
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent, Encoding.UTF8, contentType)
            });

        return handlerMock;
    }

    private void VerifyHttpRequest(
        Mock<HttpMessageHandler> handlerMock,
        string expectedEndpoint)
    {
        var expectedBaseUrl = $"{_defaultOptions.BaseUrl.TrimEnd('/')}/{expectedEndpoint.TrimStart('/')}";

        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().StartsWith(expectedBaseUrl) &&
                    req.RequestUri.Query.Length > 0), // Query string should be present
                ItExpr.IsAny<CancellationToken>());
    }
}
