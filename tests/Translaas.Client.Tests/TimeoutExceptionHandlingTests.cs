using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Translaas.Models.Errors;

namespace Translaas.Client.Tests;

public class TimeoutExceptionHandlingTests
{
    private readonly TranslaasClientOptions _defaultOptions = new()
    {
        ApiKey = "test-api-key",
        BaseUrl = "https://api.test.com/api",
        Timeout = TimeSpan.FromSeconds(5)
    };

    private Mock<HttpMessageHandler> CreateMockHttpMessageHandlerThatTimesOut()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async (HttpRequestMessage request, CancellationToken cancellationToken) =>
            {
                // Simulate a timeout by waiting longer than the timeout
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            });

        return handlerMock;
    }

    private Mock<HttpMessageHandler> CreateMockHttpMessageHandlerThatThrowsTaskCanceledException()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout of 5 seconds elapsing."));

        return handlerMock;
    }

    [Fact]
    public async Task GetEntryAsync_ShouldThrowTranslaasApiException_WhenRequestTimesOut()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandlerThatThrowsTaskCanceledException();
        var httpClient = new HttpClient(handlerMock.Object)
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        var client = new TranslaasClient(httpClient, _defaultOptions);
        var cancellationToken = new CancellationTokenSource().Token; // Not canceled

        // Act
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en", cancellationToken: cancellationToken));

        // Assert
        exception.Should().NotBeNull();
        exception.StatusCode.Should().Be(HttpStatusCode.RequestTimeout);
        exception.Message.Should().Contain("timed out");
        exception.Message.Should().Contain("5");
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowTranslaasApiException_WhenRequestTimesOut()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandlerThatThrowsTaskCanceledException();
        var httpClient = new HttpClient(handlerMock.Object)
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        var client = new TranslaasClient(httpClient, _defaultOptions);
        var cancellationToken = new CancellationTokenSource().Token; // Not canceled

        // Act
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetGroupAsync("project", "ui", "en", cancellationToken: cancellationToken));

        // Assert
        exception.Should().NotBeNull();
        exception.StatusCode.Should().Be(HttpStatusCode.RequestTimeout);
        exception.Message.Should().Contain("timed out");
        exception.Message.Should().Contain("5");
    }

    [Fact]
    public async Task GetProjectAsync_ShouldThrowTranslaasApiException_WhenRequestTimesOut()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandlerThatThrowsTaskCanceledException();
        var httpClient = new HttpClient(handlerMock.Object)
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        var client = new TranslaasClient(httpClient, _defaultOptions);
        var cancellationToken = new CancellationTokenSource().Token; // Not canceled

        // Act
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetProjectAsync("project", "en", cancellationToken: cancellationToken));

        // Assert
        exception.Should().NotBeNull();
        exception.StatusCode.Should().Be(HttpStatusCode.RequestTimeout);
        exception.Message.Should().Contain("timed out");
        exception.Message.Should().Contain("5");
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldThrowTranslaasApiException_WhenRequestTimesOut()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandlerThatThrowsTaskCanceledException();
        var httpClient = new HttpClient(handlerMock.Object)
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        var client = new TranslaasClient(httpClient, _defaultOptions);
        var cancellationToken = new CancellationTokenSource().Token; // Not canceled

        // Act
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetProjectLocalesAsync("project", cancellationToken: cancellationToken));

        // Assert
        exception.Should().NotBeNull();
        exception.StatusCode.Should().Be(HttpStatusCode.RequestTimeout);
        exception.Message.Should().Contain("timed out");
        exception.Message.Should().Contain("5");
    }

    [Fact]
    public async Task GetEntryAsync_ShouldNotThrowTimeoutException_WhenCancellationTokenIsCanceled()
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
        // Should throw TaskCanceledException, not TranslaasApiException
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => client.GetEntryAsync("ui", "greeting", "en", cancellationToken: cancellationTokenSource.Token));
    }

    [Fact]
    public async Task GetEntryAsync_ShouldIncludeTimeoutValueInExceptionMessage()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api",
            Timeout = TimeSpan.FromSeconds(10)
        };

        var handlerMock = CreateMockHttpMessageHandlerThatThrowsTaskCanceledException();
        var httpClient = new HttpClient(handlerMock.Object)
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        var client = new TranslaasClient(httpClient, options);
        var cancellationToken = new CancellationTokenSource().Token; // Not canceled

        // Act
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en", cancellationToken: cancellationToken));

        // Assert
        exception.Message.Should().Contain("10");
    }
}
