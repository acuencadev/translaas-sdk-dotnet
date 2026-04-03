using System.Net;
using System.Text;

using FluentAssertions;

using Moq;
using Moq.Protected;

using Translaas.Models.Errors;

namespace Translaas.Client.Tests;

public class GetGroupAsyncTests
{
    private readonly TranslaasClientOptions _defaultOptions;

    public GetGroupAsyncTests()
    {
        _defaultOptions = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
        };
    }

    [Fact]
    public async Task GetGroupAsync_ShouldReturnTranslationGroup_WhenRequestSucceeds()
    {
        // Arrange
        var jsonResponse = "{\"entry1\":\"Translation 1\",\"entry2\":\"Translation 2\"}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetGroupAsync("my-project", "ui", "en");

        // Assert
        result.Should().NotBeNull();
        result.Entries.Should().HaveCount(2);
        result.GetValue("entry1").Should().Be("Translation 1");
        result.GetValue("entry2").Should().Be("Translation 2");
        VerifyHttpRequest(handlerMock, "/sdk/v1/translations/group");
    }

    [Fact]
    public async Task GetGroupAsync_ShouldIncludeFormat_WhenFormatIsProvided()
    {
        // Arrange
        var jsonResponse = "{\"entry1\":\"Translation 1\"}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetGroupAsync("my-project", "ui", "en", format: "json");

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.Query.Contains("format=json")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetGroupAsync_ShouldNotIncludeFormat_WhenFormatIsNull()
    {
        // Arrange
        var jsonResponse = "{\"entry1\":\"Translation 1\"}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetGroupAsync("my-project", "ui", "en", format: null);

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    !req.RequestUri.Query.Contains("format=")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowTranslaasApiException_WhenApiReturns404()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, "Not Found", "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetGroupAsync("my-project", "nonexistent", "en");

        // Assert
        await act.Should().ThrowAsync<TranslaasApiException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowTranslaasApiException_WhenApiReturns500()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError, "Internal Server Error", "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetGroupAsync("my-project", "ui", "en");

        // Assert
        await act.Should().ThrowAsync<TranslaasApiException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowArgumentNullException_WhenProjectIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetGroupAsync(null!, "ui", "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "project");
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowArgumentNullException_WhenGroupIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetGroupAsync("my-project", null!, "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "group");
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowArgumentNullException_WhenLangIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetGroupAsync("my-project", "ui", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "lang");
    }

    [Fact]
    public async Task GetGroupAsync_ShouldPassCancellationToken_ToHttpClient()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var jsonResponse = "{\"entry1\":\"Translation 1\"}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetGroupAsync("my-project", "ui", "en", cancellationToken: cancellationToken);

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetGroupAsync_ShouldSetApiKeyHeader()
    {
        // Arrange
        var jsonResponse = "{\"entry1\":\"Translation 1\"}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetGroupAsync("my-project", "ui", "en");

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
    public async Task GetGroupAsync_ShouldUseGetMethod()
    {
        // Arrange
        var jsonResponse = "{\"entry1\":\"Translation 1\"}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetGroupAsync("my-project", "ui", "en");

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetGroupAsync_ShouldUseQueryStringParameters()
    {
        // Arrange
        var jsonResponse = "{\"entry1\":\"Translation 1\"}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetGroupAsync("my-project", "ui", "en");

        // Assert - Verify query string parameters are used (not JSON body)
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.Query.Contains("project=my-project") &&
                    req.RequestUri.Query.Contains("group=ui") &&
                    req.RequestUri.Query.Contains("lang=en") &&
                    req.Content == null), // GET requests with query strings don't have content
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetGroupAsync_ShouldDeserializeEmptyGroup_WhenResponseIsEmpty()
    {
        // Arrange
        var jsonResponse = "{}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetGroupAsync("my-project", "ui", "en");

        // Assert
        result.Should().NotBeNull();
        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGroupAsync_ShouldDeserializeComplexJson_WhenResponseHasNestedStructures()
    {
        // Arrange
        var jsonResponse = "{\"entry1\":\"Value 1\",\"entry2\":123,\"entry3\":true}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetGroupAsync("my-project", "ui", "en");

        // Assert
        result.Should().NotBeNull();
        result.Entries.Should().HaveCount(3);
        result.GetValue("entry1").Should().Be("Value 1");
        result.Entries["entry2"].GetInt32().Should().Be(123);
        result.Entries["entry3"].GetBoolean().Should().Be(true);
    }

    [Fact]
    public async Task GetGroupAsync_ShouldReturnEmptyGroup_WhenApiReturns204NoContent()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NoContent, string.Empty, "application/json");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetGroupAsync("my-project", "nonexistent", "en");

        // Assert
        result.Should().NotBeNull();
        result.Entries.Should().BeEmpty(); // Client returns empty group when 204 No Content
        VerifyHttpRequest(handlerMock, "/sdk/v1/translations/group");
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowArgumentNullException_WhenProjectIsEmptyString()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetGroupAsync(string.Empty, "ui", "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "project");
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowArgumentNullException_WhenGroupIsEmptyString()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetGroupAsync("my-project", string.Empty, "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "group");
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowArgumentNullException_WhenLangIsEmptyString()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetGroupAsync("my-project", "ui", string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "lang");
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
