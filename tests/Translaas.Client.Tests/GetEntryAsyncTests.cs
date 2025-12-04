using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;
using Moq.Protected;

using Translaas.Models.Errors;
using Translaas.Models.Requests;

namespace Translaas.Client.Tests;

public class GetEntryAsyncTests
{
    private readonly TranslaasClientOptions _defaultOptions;

    public GetEntryAsyncTests()
    {
        _defaultOptions = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
        };
    }

    [Fact]
    public async Task GetEntryAsync_ShouldReturnTranslationText_WhenRequestSucceeds()
    {
        // Arrange
        var expectedText = "Hello, World!";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, expectedText);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetEntryAsync("ui", "greeting", "en");

        // Assert
        result.Should().Be(expectedText);
        VerifyHttpRequest(handlerMock, "/api/translations/text");
    }

    [Fact]
    public async Task GetEntryAsync_ShouldIncludeNumber_WhenNumberIsProvided()
    {
        // Arrange
        var expectedText = "5 items";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, expectedText);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetEntryAsync("ui", "items", "en", number: 5);

        // Assert
        result.Should().Be(expectedText);
        VerifyHttpRequest(handlerMock, "/api/translations/text");
        
        // Verify number is included in request body
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Content != null && 
                    req.Content.ReadAsStringAsync().Result.Contains("\"n\":5")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetEntryAsync_ShouldNotIncludeNumber_WhenNumberIsNull()
    {
        // Arrange
        var expectedText = "items";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, expectedText);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetEntryAsync("ui", "items", "en", number: null);

        // Assert
        result.Should().Be(expectedText);
        VerifyHttpRequest(handlerMock, "/api/translations/text");
        
        // Verify number is null in request body
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Content != null && 
                    req.Content.ReadAsStringAsync().Result.Contains("\"n\":null")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetEntryAsync_ShouldThrowTranslaasApiException_WhenApiReturns404()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, "Not Found");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetEntryAsync("ui", "nonexistent", "en");

        // Assert
        await act.Should().ThrowAsync<TranslaasApiException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEntryAsync_ShouldThrowTranslaasApiException_WhenApiReturns500()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError, "Internal Server Error");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetEntryAsync("ui", "entry", "en");

        // Assert
        await act.Should().ThrowAsync<TranslaasApiException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetEntryAsync_ShouldThrowArgumentNullException_WhenGroupIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetEntryAsync(null!, "entry", "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "group");
    }

    [Fact]
    public async Task GetEntryAsync_ShouldThrowArgumentNullException_WhenEntryIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetEntryAsync("ui", null!, "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "entry");
    }

    [Fact]
    public async Task GetEntryAsync_ShouldThrowArgumentNullException_WhenLangIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.GetEntryAsync("ui", "entry", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "lang");
    }

    [Fact]
    public async Task GetEntryAsync_ShouldPassCancellationToken_ToHttpClient()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, "test");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetEntryAsync("ui", "entry", "en", cancellationToken: cancellationToken);

        // Assert
        // Verify that SendAsync was called (which means cancellation token was passed through)
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetEntryAsync_ShouldSetApiKeyHeader()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, "test");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetEntryAsync("ui", "entry", "en");

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
    public async Task GetEntryAsync_ShouldUseGetMethod()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, "test");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetEntryAsync("ui", "entry", "en");

        // Assert
        handlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetEntryAsync_ShouldSetJsonContentType()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, "test");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        await client.GetEntryAsync("ui", "entry", "en");

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

    private Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
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
                Content = new StringContent(responseContent, Encoding.UTF8, "text/plain")
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
