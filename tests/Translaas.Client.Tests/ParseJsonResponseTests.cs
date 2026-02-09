using System.Net;
using System.Text;

using FluentAssertions;
using Moq;
using Moq.Protected;
using Translaas.Models.Errors;

namespace Translaas.Client.Tests;

public class ParseJsonResponseTests
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
    public async Task ParseJsonResponse_ShouldDeserializeValidJson_WhenResponseIsSuccessful()
    {
        // Arrange
        var jsonResponse = """{"button.save":"Save","button.cancel":"Cancel"}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetGroupAsync("my-project", "ui", "en");

        // Assert
        result.Should().NotBeNull();
        result.Entries.Should().HaveCount(2);
        result.GetValue("button.save").Should().Be("Save");
        result.GetValue("button.cancel").Should().Be("Cancel");
    }

    [Fact]
    public async Task ParseJsonResponse_ShouldThrowTranslaasApiException_WhenDeserializationFails()
    {
        // Arrange
        var invalidJson = "This is not valid JSON";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, invalidJson);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetGroupAsync("my-project", "ui", "en"));

        exception.Message.Should().Contain("Failed to deserialize response");
        exception.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.ResponseContent.Should().Be(invalidJson);
    }

    [Fact]
    public async Task ParseJsonResponse_ShouldThrowTranslaasApiException_WhenResponseIsNull()
    {
        // Arrange
        var jsonResponse = "null";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetGroupAsync("my-project", "ui", "en"));

        exception.Message.Should().Be("Failed to deserialize response from API.");
        exception.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.ResponseContent.Should().Be(jsonResponse);
    }

    [Fact]
    public async Task ParseJsonResponse_ShouldHandleEmptyJsonObject()
    {
        // Arrange
        var jsonResponse = "{}";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetGroupAsync("my-project", "ui", "en");

        // Assert
        result.Should().NotBeNull();
        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseJsonResponse_ShouldWorkWithGetProjectAsync()
    {
        // Arrange
        var jsonResponse = """{"ui":{"button.save":"Save"},"errors":{"error.generic":"An error occurred"}}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetProjectAsync("my-project", "en");

        // Assert
        result.Should().NotBeNull();
        result.Groups.Should().HaveCount(2);
        result.GetGroup("ui")?.GetValue("button.save").Should().Be("Save");
    }

    [Fact]
    public async Task ParseJsonResponse_ShouldWorkWithGetProjectLocalesAsync()
    {
        // Arrange
        var jsonResponse = """{"locales":["en","fr","es"]}""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act
        var result = await client.GetProjectLocalesAsync("my-project");

        // Assert
        result.Should().NotBeNull();
        result.Locales.Should().HaveCount(3);
        result.Locales.Should().Contain("en");
        result.Locales.Should().Contain("fr");
        result.Locales.Should().Contain("es");
    }

    [Fact]
    public async Task ParseJsonResponse_ShouldRespectCancellationToken()
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
            () => client.GetGroupAsync("project", "ui", "en", cancellationToken: cancellationTokenSource.Token));
    }

    [Fact]
    public async Task ParseJsonResponse_ShouldHandleMalformedJsonWithPartialData()
    {
        // Arrange
        var invalidJson = """{"button.save":"Save","button.cancel":""";
        var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK, invalidJson);
        var httpClient = new HttpClient(handlerMock.Object);
        var client = new TranslaasClient(httpClient, _defaultOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetGroupAsync("my-project", "ui", "en"));

        exception.Message.Should().Contain("Failed to deserialize response");
        exception.InnerException.Should().NotBeNull();
        exception.ResponseContent.Should().Be(invalidJson);
    }
}
