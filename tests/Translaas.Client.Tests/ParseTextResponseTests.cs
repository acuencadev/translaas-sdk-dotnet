using System.Net;
using System.Text;

using FluentAssertions;
using Moq;
using Moq.Protected;

namespace Translaas.Client.Tests;

public class ParseTextResponseTests
{
    private readonly TranslaasClientOptions _defaultOptions = new()
    {
        ApiKey = "test-api-key",
        BaseUrl = "https://api.test.com"
    };

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

    [Fact]
    public async Task ParseTextResponse_ShouldReturnTextContent_WhenResponseIsSuccessful()
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
    }

    [Fact]
    public async Task ParseTextResponse_ShouldReturnEmptyString_WhenResponseIsEmpty()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, string.Empty);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetEntryAsync("ui", "greeting", "en");

        // Assert
        result.Should().Be(string.Empty);
    }

    [Fact]
    public async Task ParseTextResponse_ShouldHandleWhitespaceOnlyResponse()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, "   ");
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetEntryAsync("ui", "greeting", "en");

        // Assert
        result.Should().Be("   ");
    }

    [Fact]
    public async Task ParseTextResponse_ShouldHandleMultilineText()
    {
        // Arrange
        var expectedText = "Line 1\nLine 2\nLine 3";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, expectedText);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetEntryAsync("ui", "greeting", "en");

        // Assert
        result.Should().Be(expectedText);
    }

    [Fact]
    public async Task ParseTextResponse_ShouldRespectCancellationToken()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("A task was canceled."));

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => client.GetEntryAsync("ui", "greeting", "en", cancellationToken: cancellationTokenSource.Token));
    }
}
