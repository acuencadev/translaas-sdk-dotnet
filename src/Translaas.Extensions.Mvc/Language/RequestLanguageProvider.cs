using System;
using System.Globalization;

using Microsoft.AspNetCore.Http;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Routing;
#endif

using Translaas.Extensions.DependencyInjection;

namespace Translaas.Extensions.Mvc;

/// <summary>
/// Language provider that resolves language from HTTP request context.
/// </summary>
/// <remarks>
/// This provider checks configured sources (route, query string, header, cookie, Accept-Language)
/// in order and returns the first match. Returns null if not in HTTP context or no source has a value.
/// </remarks>
public class RequestLanguageProvider : ILanguageProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly RequestLanguageOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLanguageProvider"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="options">The request language options.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when httpContextAccessor or options is null.</exception>
    public RequestLanguageProvider(IHttpContextAccessor httpContextAccessor, RequestLanguageOptions options)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new System.ArgumentNullException(nameof(httpContextAccessor));
        _options = options ?? throw new System.ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public string? GetLanguage()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            // Not in HTTP context - gracefully return null
            return null;
        }

        // Check sources in configured order
        foreach (var source in _options.Sources)
        {
            var lang = GetLanguageFromSource(httpContext, source);
            if (!string.IsNullOrWhiteSpace(lang))
            {
                return lang;
            }
        }

        return null;
    }

    private string? GetLanguageFromSource(HttpContext httpContext, RequestLanguageSource source)
    {
        return source switch
        {
            RequestLanguageSource.Route => GetLanguageFromRoute(httpContext),
            RequestLanguageSource.QueryString => GetLanguageFromQueryString(httpContext),
            RequestLanguageSource.Header => GetLanguageFromHeader(httpContext),
            RequestLanguageSource.Cookie => GetLanguageFromCookie(httpContext),
            RequestLanguageSource.AcceptLanguage => GetLanguageFromAcceptLanguage(httpContext),
            _ => null
        };
    }

    private string? GetLanguageFromRoute(HttpContext httpContext)
    {
#if NETSTANDARD2_0
        // For netstandard2.0, RouteValues is accessed via IRoutingFeature
        var routingFeature = httpContext.Features.Get<Microsoft.AspNetCore.Routing.IRoutingFeature>();
        if (routingFeature?.RouteData?.Values != null && 
            routingFeature.RouteData.Values.TryGetValue(_options.RouteParameterName, out var routeValue))
        {
            return routeValue?.ToString();
        }
#else
        if (httpContext.Request.RouteValues.TryGetValue(_options.RouteParameterName, out var routeValue))
        {
            return routeValue?.ToString();
        }
#endif
        return null;
    }

    private string? GetLanguageFromQueryString(HttpContext httpContext)
    {
        if (httpContext.Request.Query.TryGetValue(_options.QueryParameterName, out var queryValue))
        {
            return queryValue.ToString();
        }

        return null;
    }

    private string? GetLanguageFromHeader(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue(_options.HeaderName, out var headerValue))
        {
            return headerValue.ToString();
        }

        return null;
    }

    private string? GetLanguageFromCookie(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue(_options.CookieName, out var cookieValue))
        {
            return cookieValue;
        }

        return null;
    }

    private string? GetLanguageFromAcceptLanguage(HttpContext httpContext)
    {
        var acceptLanguageHeader = httpContext.Request.Headers["Accept-Language"].ToString();
        if (string.IsNullOrWhiteSpace(acceptLanguageHeader))
        {
            return null;
        }

        // Parse Accept-Language header (e.g., "en-US,en;q=0.9,fr;q=0.8")
        // Return the first language code (highest quality)
        var parts = acceptLanguageHeader.Split(',');
        string? firstLanguage = null;

        foreach (var part in parts)
        {
            var trimmed = part.Split(';')[0].Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                firstLanguage = trimmed;
                break;
            }
        }

        if (firstLanguage == null)
        {
            return null;
        }

        // Try to parse as culture to extract two-letter code
        try
        {
            var culture = CultureInfo.GetCultureInfo(firstLanguage);
            return culture.TwoLetterISOLanguageName;
        }
        catch (CultureNotFoundException)
        {
            // If parsing fails, return as-is (might be a custom code)
            return firstLanguage;
        }
    }
}
