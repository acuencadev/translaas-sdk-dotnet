using System.Collections.Generic;

namespace Translaas.Extensions.Mvc;

/// <summary>
/// Configuration options for the HTTP request-based language provider.
/// </summary>
public class RequestLanguageOptions
{
    /// <summary>
    /// Route parameter name to check (e.g., "/api/{lang}/products").
    /// Default: "lang"
    /// </summary>
    public string RouteParameterName { get; set; } = "lang";
    
    /// <summary>
    /// Query string parameter name to check (e.g., "?lang=en").
    /// Default: "lang"
    /// </summary>
    public string QueryParameterName { get; set; } = "lang";
    
    /// <summary>
    /// Custom header name to check.
    /// Default: "X-Language"
    /// </summary>
    public string HeaderName { get; set; } = "X-Language";
    
    /// <summary>
    /// Cookie name to check.
    /// Default: "lang"
    /// </summary>
    public string CookieName { get; set; } = "lang";
    
    /// <summary>
    /// Ordered list of sources to check. First match wins.
    /// Default: [Route, QueryString, Header, Cookie]
    /// </summary>
    public List<RequestLanguageSource> Sources { get; set; } = new()
    {
        RequestLanguageSource.Route,
        RequestLanguageSource.QueryString,
        RequestLanguageSource.Header,
        RequestLanguageSource.Cookie
    };
}
