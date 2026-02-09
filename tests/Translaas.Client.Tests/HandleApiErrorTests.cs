using System.Net;
using System.Text;

using FluentAssertions;
using Moq;
using Moq.Protected;
using Translaas.Models.Errors;

namespace Translaas.Client.Tests;

public class HandleApiErrorTests
{
    private readonly TranslaasClientOptions _defaultOptions = new()
    {
        ApiKey = "test-api-key",
        BaseUrl = "https://api.test.com/api"
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
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        return handlerMock;
    }

    [Fact]
    public async Task HandleApiError_ShouldParseJsonErrorResponse_WhenValidJson()
    {
        // Arrange
        var errorResponse = """{"message":"Invalid API Key","code":"AUTH_001"}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.Unauthorized, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        exception.Message.Should().Contain("Invalid API Key");
        exception.Message.Should().Contain("[AUTH_001]");
        exception.ResponseContent.Should().Be(errorResponse);
    }

    [Fact]
    public async Task HandleApiError_ShouldHandleErrorWithoutCode_WhenCodeIsMissing()
    {
        // Arrange
        var errorResponse = """{"message":"Resource not found"}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        exception.Message.Should().Be("Resource not found");
        exception.Message.Should().NotContain("[");
        exception.ResponseContent.Should().Be(errorResponse);
    }

    [Fact]
    public async Task HandleApiError_ShouldHandleMalformedJson_WhenResponseIsNotValidJson()
    {
        // Arrange
        var errorResponse = "This is not valid JSON";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.BadRequest, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Message.Should().Contain("API request failed with status code BadRequest");
        exception.ResponseContent.Should().Be(errorResponse);
    }

    [Fact]
    public async Task HandleApiError_ShouldHandleEmptyResponse_WhenResponseIsEmpty()
    {
        // Arrange
        var errorResponse = string.Empty;
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Message.Should().Contain("API request failed with status code InternalServerError");
        exception.ResponseContent.Should().Be(errorResponse);
    }

    [Fact]
    public async Task HandleApiError_ShouldHandle400BadRequest_WhenStatusIsBadRequest()
    {
        // Arrange
        var errorResponse = """{"message":"Invalid request parameters","code":"VALIDATION_001"}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.BadRequest, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Message.Should().Contain("Invalid request parameters");
        exception.Message.Should().Contain("[VALIDATION_001]");
    }

    [Fact]
    public async Task HandleApiError_ShouldHandle401Unauthorized_WhenStatusIsUnauthorized()
    {
        // Arrange
        var errorResponse = """{"message":"Authentication failed","code":"AUTH_002"}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.Unauthorized, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        exception.Message.Should().Contain("Authentication failed");
        exception.Message.Should().Contain("[AUTH_002]");
    }

    [Fact]
    public async Task HandleApiError_ShouldHandle404NotFound_WhenStatusIsNotFound()
    {
        // Arrange
        var errorResponse = """{"message":"Translation entry not found","code":"NOT_FOUND_001"}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        exception.Message.Should().Contain("Translation entry not found");
        exception.Message.Should().Contain("[NOT_FOUND_001]");
    }

    [Fact]
    public async Task HandleApiError_ShouldHandle500InternalServerError_WhenStatusIsInternalServerError()
    {
        // Arrange
        var errorResponse = """{"message":"Internal server error occurred","code":"SERVER_001"}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Message.Should().Contain("Internal server error occurred");
        exception.Message.Should().Contain("[SERVER_001]");
    }

    [Fact]
    public async Task HandleApiError_ShouldWorkWithGetGroupAsync_WhenErrorOccurs()
    {
        // Arrange
        var errorResponse = """{"message":"Group not found","code":"GROUP_NOT_FOUND"}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetGroupAsync("project", "ui", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        exception.Message.Should().Contain("Group not found");
        exception.Message.Should().Contain("[GROUP_NOT_FOUND]");
    }

    [Fact]
    public async Task HandleApiError_ShouldWorkWithGetProjectAsync_WhenErrorOccurs()
    {
        // Arrange
        var errorResponse = """{"message":"Project not found","code":"PROJECT_NOT_FOUND"}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetProjectAsync("project", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        exception.Message.Should().Contain("Project not found");
        exception.Message.Should().Contain("[PROJECT_NOT_FOUND]");
    }

    [Fact]
    public async Task HandleApiError_ShouldWorkWithGetProjectLocalesAsync_WhenErrorOccurs()
    {
        // Arrange
        var errorResponse = """{"message":"Project locales not found","code":"LOCALES_NOT_FOUND"}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetProjectLocalesAsync("project"));

        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        exception.Message.Should().Contain("Project locales not found");
        exception.Message.Should().Contain("[LOCALES_NOT_FOUND]");
    }

    [Fact]
    public async Task HandleApiError_ShouldRespectCancellationToken_WhenCancellationRequested()
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

    [Fact]
    public async Task HandleApiError_ShouldHandleErrorWithOnlyCode_WhenMessageIsMissing()
    {
        // Arrange
        var errorResponse = """{"code":"ERROR_CODE_001"}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.BadRequest, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Message.Should().Contain("API request failed with status code BadRequest");
        exception.ResponseContent.Should().Be(errorResponse);
    }

    [Fact]
    public async Task HandleApiError_ShouldHandleWhitespaceOnlyResponse_WhenResponseIsWhitespace()
    {
        // Arrange
        var errorResponse = "   ";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.BadRequest, errorResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Message.Should().Contain("API request failed with status code BadRequest");
        exception.ResponseContent.Should().Be(errorResponse);
    }
}
