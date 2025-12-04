using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;
using Moq.Protected;

using Translaas.Models.Errors;
using Translaas.Models.Requests;
using Translaas.Models.Responses;

namespace Translaas.Client.Tests;

public class GetProjectAsyncTests
{
    private readonly TranslaasClientOptions _defaultOptions;

    public GetProjectAsyncTests()
    {
        _defaultOptions = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
        };
    }

    [Fact]
    public async Task GetProjectAsync_ShouldReturnTranslationProject_WhenRequestSucceeds()
    {
        // Arrange
        var jsonResponse = "{\"group1\":{\"entry1\":\"Translation 1\",\"entry2\":\"Translation 2\"},\"group2\":{\"entry3\":\"Translation 3\"}}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetProjectAsync("my-project", "en");

        // Assert
        result.Should().NotBeNull();
        result.Groups.Should().HaveCount(2);
        result.Groups.Should().ContainKey("group1");
        result.Groups.Should().ContainKey("group2");
        
        var group1 = result.GetGroup("group1");
        group1.Should().NotBeNull();
        group1!.GetValue("entry1").Should().Be("Translation 1");
        group1.GetValue("entry2").Should().Be("Translation 2");
        
        var group2 = result.GetGroup("group2");
        group2.Should().NotBeNull();
        group2!.GetValue("entry3").Should().Be("Translation 3");
        
        VerifyHttpRequest(handlerMock, "/api/translations/project");
    }

    [Fact]
    public async Task GetProjectAsync_ShouldIncludeFormat_WhenFormatIsProvided()
    {
        // Arrange
        var jsonResponse = "{\"group1\":{\"entry1\":\"Translation 1\"}}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetProjectAsync("my-project", "en", format: "json");

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content != null &&
                    req.Content.ReadAsStringAsync().Result.Contains("\"format\":\"json\"")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectAsync_ShouldNotIncludeFormat_WhenFormatIsNull()
    {
        // Arrange
        var jsonResponse = "{\"group1\":{\"entry1\":\"Translation 1\"}}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetProjectAsync("my-project", "en", format: null);

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content != null &&
                    req.Content.ReadAsStringAsync().Result.Contains("\"format\":null")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectAsync_ShouldThrowTranslaasApiException_WhenApiReturns404()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, "Not Found", "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetProjectAsync("nonexistent-project", "en");

        // Assert
        await act.Should().ThrowAsync<TranslaasApiException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProjectAsync_ShouldThrowTranslaasApiException_WhenApiReturns500()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError, "Internal Server Error", "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetProjectAsync("my-project", "en");

        // Assert
        await act.Should().ThrowAsync<TranslaasApiException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetProjectAsync_ShouldThrowArgumentNullException_WhenProjectIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetProjectAsync(null!, "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "project");
    }

    [Fact]
    public async Task GetProjectAsync_ShouldThrowArgumentNullException_WhenLangIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetProjectAsync("my-project", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "lang");
    }

    [Fact]
    public async Task GetProjectAsync_ShouldPassCancellationToken_ToHttpClient()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var jsonResponse = "{\"group1\":{\"entry1\":\"Translation 1\"}}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetProjectAsync("my-project", "en", cancellationToken: cancellationToken);

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectAsync_ShouldSetApiKeyHeader()
    {
        // Arrange
        var jsonResponse = "{\"group1\":{\"entry1\":\"Translation 1\"}}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetProjectAsync("my-project", "en");

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
    public async Task GetProjectAsync_ShouldUseGetMethod()
    {
        // Arrange
        var jsonResponse = "{\"group1\":{\"entry1\":\"Translation 1\"}}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetProjectAsync("my-project", "en");

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectAsync_ShouldSetJsonContentType()
    {
        // Arrange
        var jsonResponse = "{\"group1\":{\"entry1\":\"Translation 1\"}}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetProjectAsync("my-project", "en");

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content != null &&
                    req.Content.Headers.ContentType != null &&
                    req.Content.Headers.ContentType.MediaType == "application/json"),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectAsync_ShouldDeserializeEmptyProject_WhenResponseIsEmpty()
    {
        // Arrange
        var jsonResponse = "{}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetProjectAsync("my-project", "en");

        // Assert
        result.Should().NotBeNull();
        result.Groups.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProjectAsync_ShouldThrowTranslaasApiException_WhenDeserializationFails()
    {
        // Arrange
        var invalidJson = "{invalid json}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, invalidJson, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetProjectAsync("my-project", "en");

        // Assert
        await act.Should().ThrowAsync<TranslaasApiException>()
            .Where(ex => ex.InnerException is JsonException);
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
        var expectedUrl = $"{_defaultOptions.BaseUrl.TrimEnd('/')}/{expectedEndpoint.TrimStart('/')}";

        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>());
    }
}
