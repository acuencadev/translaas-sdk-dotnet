using FluentAssertions;

using Translaas.Models.Requests;

namespace Translaas.Client.Tests;

public class HttpRequestBuildingTests
{
    [Fact]
    public void BuildEndpointUrl_ShouldCombineBaseUrlWithEndpoint()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.test.com/api"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);

        // Act
        var url = client.BuildEndpointUrl("translations/text");

        // Assert
        url.Should().Be("https://api.test.com/api/translations/text");
    }

    [Fact]
    public void BuildEndpointUrl_ShouldHandleBaseUrlWithTrailingSlash()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.test.com/api/"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);

        // Act
        var url = client.BuildEndpointUrl("translations/text");

        // Assert
        url.Should().Be("https://api.test.com/api/translations/text");
    }

    [Fact]
    public void BuildEndpointUrl_ShouldHandleEndpointWithLeadingSlash()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.test.com/api"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);

        // Act
        var url = client.BuildEndpointUrl("/translations/text");

        // Assert
        url.Should().Be("https://api.test.com/api/translations/text");
    }

    [Fact]
    public void BuildEndpointUrl_ShouldHandleBothTrailingAndLeadingSlashes()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.test.com/api/"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);

        // Act
        var url = client.BuildEndpointUrl("/translations/text");

        // Assert
        url.Should().Be("https://api.test.com/api/translations/text");
    }

    [Fact]
    public void BuildGetRequest_ShouldCreateGetRequest()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);
        var requestModel = new GetTranslationRequest
        {
            Group = "ui",
            Entry = "button.save",
            Lang = "en"
        };

        // Act
        var request = client.BuildGetRequest("translations/text", requestModel);

        // Assert
        request.Should().NotBeNull();
        request.Method.Should().Be(HttpMethod.Get);
        request.RequestUri!.ToString().Should().Be("https://api.test.com/api/translations/text");
    }

    [Fact]
    public void BuildGetRequest_ShouldSetApiKeyHeader()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);
        var requestModel = new GetTranslationRequest
        {
            Group = "ui",
            Entry = "button.save",
            Lang = "en"
        };

        // Act
        var request = client.BuildGetRequest("translations/text", requestModel);

        // Assert
        request.Headers.GetValues("X-Api-Key").Should().Contain("test-api-key");
    }

    [Fact]
    public void BuildGetRequest_ShouldSetJsonContentType()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);
        var requestModel = new GetTranslationRequest
        {
            Group = "ui",
            Entry = "button.save",
            Lang = "en"
        };

        // Act
        var request = client.BuildGetRequest("translations/text", requestModel);

        // Assert
        request.Content.Should().NotBeNull();
        request.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task BuildGetRequest_ShouldSetJsonRequestBody()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);
        var requestModel = new GetTranslationRequest
        {
            Group = "ui",
            Entry = "button.save",
            Lang = "en",
            Number = 5
        };

        // Act
        var request = client.BuildGetRequest("translations/text", requestModel);

        // Assert
        var body = await request.Content!.ReadAsStringAsync();
        body.Should().Contain("\"group\":\"ui\"");
        body.Should().Contain("\"entry\":\"button.save\"");
        body.Should().Contain("\"lang\":\"en\"");
        body.Should().Contain("\"n\":5");
    }

    [Fact]
    public void BuildGetRequest_ShouldThrowWhenEndpointIsNull()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);
        var requestModel = new GetTranslationRequest();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => client.BuildGetRequest(null!, requestModel));
    }

    [Fact]
    public void BuildGetRequest_ShouldThrowWhenRequestModelIsNull()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => client.BuildGetRequest<GetTranslationRequest>("translations/text", null!));
    }
}
