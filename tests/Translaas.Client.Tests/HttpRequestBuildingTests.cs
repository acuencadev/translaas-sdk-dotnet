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
            BaseUrl = "https://api.test.com"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);

        // Act
        var url = client.BuildEndpointUrl("sdk/v1/translations/text");

        // Assert
        url.Should().Be("https://api.test.com/sdk/v1/translations/text");
    }

    [Fact]
    public void BuildEndpointUrl_ShouldHandleBaseUrlWithTrailingSlash()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.test.com/"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);

        // Act
        var url = client.BuildEndpointUrl("sdk/v1/translations/text");

        // Assert
        url.Should().Be("https://api.test.com/sdk/v1/translations/text");
    }

    [Fact]
    public void BuildEndpointUrl_ShouldHandleEndpointWithLeadingSlash()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.test.com"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);

        // Act
        var url = client.BuildEndpointUrl("/sdk/v1/translations/text");

        // Assert
        url.Should().Be("https://api.test.com/sdk/v1/translations/text");
    }

    [Fact]
    public void BuildEndpointUrl_ShouldHandleBothTrailingAndLeadingSlashes()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.test.com/"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);

        // Act
        var url = client.BuildEndpointUrl("/sdk/v1/translations/text");

        // Assert
        url.Should().Be("https://api.test.com/sdk/v1/translations/text");
    }

    [Fact]
    public void BuildGetRequest_ShouldCreateGetRequest()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
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
        var request = client.BuildGetRequest("sdk/v1/translations/text", requestModel);

        // Assert
        request.Should().NotBeNull();
        request.Method.Should().Be(HttpMethod.Get);
        request.RequestUri!.ToString().Should().StartWith("https://api.test.com/sdk/v1/translations/text");
        request.RequestUri.Query.Should().Contain("group=ui");
        request.RequestUri.Query.Should().Contain("entry=button.save");
        request.RequestUri.Query.Should().Contain("lang=en");
    }

    [Fact]
    public void BuildGetRequest_ShouldSetApiKeyHeader()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
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
        var request = client.BuildGetRequest("sdk/v1/translations/text", requestModel);

        // Assert
        request.Headers.GetValues("X-Api-Key").Should().Contain("test-api-key");
    }

    [Fact]
    public void BuildGetRequest_ShouldUseQueryStringParameters()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
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
        var request = client.BuildGetRequest("sdk/v1/translations/text", requestModel);

        // Assert
        request.Content.Should().BeNull(); // GET requests with query strings don't have content
        request.RequestUri.Should().NotBeNull();
        request.RequestUri!.Query.Should().Contain("group=ui");
        request.RequestUri.Query.Should().Contain("entry=button.save");
        request.RequestUri.Query.Should().Contain("lang=en");
    }

    [Fact]
    public void BuildGetRequest_ShouldIncludeRequestModelInQueryString()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
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
        var request = client.BuildGetRequest("sdk/v1/translations/text", requestModel);

        // Assert
        request.Content.Should().BeNull(); // GET requests with query strings don't have content
        request.RequestUri.Should().NotBeNull();
        request.RequestUri!.Query.Should().Contain("group=ui");
        request.RequestUri.Query.Should().Contain("entry=button.save");
        request.RequestUri.Query.Should().Contain("lang=en");
        request.RequestUri.Query.Should().Contain("n=5");
    }

    [Fact]
    public void BuildGetRequest_ShouldThrowWhenEndpointIsNull()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
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
            BaseUrl = "https://api.test.com"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => client.BuildGetRequest<GetTranslationRequest>("sdk/v1/translations/text", null!));
    }

    [Fact]
    public void BuildGetRequest_ShouldAppendQueryStringParameters_WhenParametersProvided()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);
        var requestModel = new GetTranslationRequest
        {
            Group = "ui",
            Entry = "button.save",
            Lang = "en"
        };
        var parameters = new Dictionary<string, string>
        {
            { "userName", "John" },
            { "N", "5" }
        };

        // Act
        var request = client.BuildGetRequest("sdk/v1/translations/text", requestModel, parameters);

        // Assert
        request.Should().NotBeNull();
        request.RequestUri!.Query.Should().Contain("userName=John");
        request.RequestUri.Query.Should().Contain("N=5"); // Parameter name preserves case from dictionary
    }

    [Fact]
    public void BuildGetRequest_ShouldUrlEncodeParameterValues()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);
        var requestModel = new GetTranslationRequest
        {
            Group = "ui",
            Entry = "button.save",
            Lang = "en"
        };
        var parameters = new Dictionary<string, string>
        {
            { "userName", "John Doe" },
            { "message", "Hello & Welcome" }
        };

        // Act
        var request = client.BuildGetRequest("sdk/v1/translations/text", requestModel, parameters);

        // Assert
        request.Should().NotBeNull();
        request.RequestUri!.Query.Should().Contain("userName=John%20Doe");
        request.RequestUri.Query.Should().Contain("message=Hello%20%26%20Welcome");
    }

    [Fact]
    public void BuildGetRequest_ShouldUrlEncodeParameterNames()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);
        var requestModel = new GetTranslationRequest
        {
            Group = "ui",
            Entry = "button.save",
            Lang = "en"
        };
        var parameters = new Dictionary<string, string>
        {
            { "user_name", "John" }
        };

        // Act
        var request = client.BuildGetRequest("sdk/v1/translations/text", requestModel, parameters);

        // Assert
        request.Should().NotBeNull();
        request.RequestUri!.Query.Should().Contain("user_name=John");
    }

    [Fact]
    public void BuildGetRequest_ShouldIncludeRequestModelInQueryString_WhenParametersIsNull()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
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
        var request = client.BuildGetRequest("sdk/v1/translations/text", requestModel, null);

        // Assert
        request.Should().NotBeNull();
        request.RequestUri!.Query.Should().NotBeEmpty(); // Request model properties are always in query string
        request.RequestUri.Query.Should().Contain("group=ui");
        request.RequestUri.Query.Should().Contain("entry=button.save");
        request.RequestUri.Query.Should().Contain("lang=en");
    }

    [Fact]
    public void BuildGetRequest_ShouldIncludeRequestModelProperties_WhenParametersIsEmpty()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com"
        };
        using var httpClient = new HttpClient();
        var client = new TranslaasClient(httpClient, options);
        var requestModel = new GetTranslationRequest
        {
            Group = "ui",
            Entry = "button.save",
            Lang = "en"
        };
        var parameters = new Dictionary<string, string>();

        // Act
        var request = client.BuildGetRequest("sdk/v1/translations/text", requestModel, parameters);

        // Assert
        request.Should().NotBeNull();
        // Request model properties are always included in query string, even when parameters is empty
        request.RequestUri!.Query.Should().Contain("group=ui");
        request.RequestUri.Query.Should().Contain("entry=button.save");
        request.RequestUri.Query.Should().Contain("lang=en");
    }
}
