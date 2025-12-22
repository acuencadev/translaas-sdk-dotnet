using System.Collections.Generic;

using FluentAssertions;

using Microsoft.AspNetCore.Http;

using Moq;

using Xunit;

namespace Translaas.Extensions.Mvc.Tests.Language;

/// <summary>
/// Tests for RequestLanguageProvider.
/// </summary>
public class RequestLanguageProviderTests
{
    [Fact]
    public void GetLanguage_ReturnsNull_WhenNotInHttpContext()
    {
        // Arrange
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var options = new RequestLanguageOptions();
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetLanguage_ReturnsLanguageFromRoute_WhenConfigured()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        httpContext.Request.RouteValues["lang"] = "fr";

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var options = new RequestLanguageOptions
        {
            Sources = new List<RequestLanguageSource> { RequestLanguageSource.Route }
        };
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("fr");
    }

    [Fact]
    public void GetLanguage_ReturnsLanguageFromQueryString_WhenConfigured()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "lang", "es" }
        });

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var options = new RequestLanguageOptions
        {
            Sources = new List<RequestLanguageSource> { RequestLanguageSource.QueryString }
        };
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("es");
    }

    [Fact]
    public void GetLanguage_ReturnsLanguageFromHeader_WhenConfigured()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        httpContext.Request.Headers["X-Language"] = "de";

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var options = new RequestLanguageOptions
        {
            Sources = new List<RequestLanguageSource> { RequestLanguageSource.Header }
        };
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("de");
    }

    [Fact]
    public void GetLanguage_ReturnsLanguageFromCookie_WhenConfigured()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        httpContext.Request.Cookies = new MockRequestCookieCollection(new Dictionary<string, string> { { "lang", "it" } });

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var options = new RequestLanguageOptions
        {
            Sources = new List<RequestLanguageSource> { RequestLanguageSource.Cookie }
        };
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("it");
    }

    [Fact]
    public void GetLanguage_ReturnsLanguageFromAcceptLanguage_WhenConfigured()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "en-US,en;q=0.9,fr;q=0.8";

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var options = new RequestLanguageOptions
        {
            Sources = new List<RequestLanguageSource> { RequestLanguageSource.AcceptLanguage }
        };
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("en"); // Should extract two-letter code from "en-US"
    }

    [Fact]
    public void GetLanguage_ReturnsFirstMatch_WhenMultipleSourcesConfigured()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        httpContext.Request.RouteValues["lang"] = "fr";
        httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "lang", "es" }
        });

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var options = new RequestLanguageOptions
        {
            Sources = new List<RequestLanguageSource> 
            { 
                RequestLanguageSource.Route,
                RequestLanguageSource.QueryString
            }
        };
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("fr"); // Route comes first in Sources list
    }

    [Fact]
    public void GetLanguage_ReturnsNull_WhenNoSourceHasValue()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var options = new RequestLanguageOptions
        {
            Sources = new List<RequestLanguageSource> 
            { 
                RequestLanguageSource.Route,
                RequestLanguageSource.QueryString
            }
        };
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetLanguage_UsesCustomRouteParameterName()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        httpContext.Request.RouteValues["culture"] = "pt";

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var options = new RequestLanguageOptions
        {
            RouteParameterName = "culture",
            Sources = new List<RequestLanguageSource> { RequestLanguageSource.Route }
        };
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("pt");
    }

    [Fact]
    public void GetLanguage_UsesCustomQueryParameterName()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "locale", "ru" }
        });

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var options = new RequestLanguageOptions
        {
            QueryParameterName = "locale",
            Sources = new List<RequestLanguageSource> { RequestLanguageSource.QueryString }
        };
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("ru");
    }

    [Fact]
    public void GetLanguage_UsesCustomHeaderName()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        httpContext.Request.Headers["X-Custom-Language"] = "ja";

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var options = new RequestLanguageOptions
        {
            HeaderName = "X-Custom-Language",
            Sources = new List<RequestLanguageSource> { RequestLanguageSource.Header }
        };
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("ja");
    }

    [Fact]
    public void GetLanguage_UsesCustomCookieName()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        httpContext.Request.Cookies = new MockRequestCookieCollection(new Dictionary<string, string> { { "locale", "zh" } });

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var options = new RequestLanguageOptions
        {
            CookieName = "locale",
            Sources = new List<RequestLanguageSource> { RequestLanguageSource.Cookie }
        };
        var provider = new RequestLanguageProvider(httpContextAccessor.Object, options);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("zh");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHttpContextAccessorIsNull()
    {
        // Arrange & Act
        var options = new RequestLanguageOptions();
        var act = () => new RequestLanguageProvider(null!, options);

        // Assert
        act.Should().Throw<System.ArgumentNullException>()
            .WithParameterName("httpContextAccessor");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange & Act
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var act = () => new RequestLanguageProvider(httpContextAccessor.Object, null!);

        // Assert
        act.Should().Throw<System.ArgumentNullException>()
            .WithParameterName("options");
    }

    private static HttpContext CreateMockHttpContext()
    {
        var httpContext = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var response = new Mock<HttpResponse>();
        var headers = new HeaderDictionary();
        var routeValues = new Microsoft.AspNetCore.Routing.RouteValueDictionary();
        var query = new QueryCollection();

        request.Setup(r => r.Headers).Returns(headers);
        request.Setup(r => r.RouteValues).Returns(routeValues);
        request.Setup(r => r.Query).Returns(query);
        request.Setup(r => r.Cookies).Returns(new MockRequestCookieCollection());

        httpContext.Setup(c => c.Request).Returns(request.Object);
        httpContext.Setup(c => c.Response).Returns(response.Object);

        return httpContext.Object;
    }

    private class MockRequestCookieCollection : IRequestCookieCollection
    {
        private readonly Dictionary<string, string> _cookies;

        public MockRequestCookieCollection(Dictionary<string, string>? cookies = null)
        {
            _cookies = cookies ?? new Dictionary<string, string>();
        }

        public string? this[string key] => _cookies.TryGetValue(key, out var value) ? value : null;

        public int Count => _cookies.Count;

        public ICollection<string> Keys => _cookies.Keys;

        public bool ContainsKey(string key) => _cookies.ContainsKey(key);

        public bool TryGetValue(string key, out string? value)
        {
            var result = _cookies.TryGetValue(key, out var val);
            value = val;
            return result;
        }

        public System.Collections.IEnumerator GetEnumerator() => _cookies.GetEnumerator();

        System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, string>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>.GetEnumerator()
            => _cookies.GetEnumerator();
    }
}
