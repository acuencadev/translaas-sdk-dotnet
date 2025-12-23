namespace Translaas.Extensions.Mvc;

/// <summary>
/// Defines the sources for HTTP request-based language resolution.
/// </summary>
public enum RequestLanguageSource
{
    /// <summary>Check route data (e.g., /en/products)</summary>
    Route,
    
    /// <summary>Check query string (e.g., ?lang=en)</summary>
    QueryString,
    
    /// <summary>Check custom header (default: X-Language)</summary>
    Header,
    
    /// <summary>Check cookie value</summary>
    Cookie,
    
    /// <summary>Parse Accept-Language header (returns first preferred language)</summary>
    AcceptLanguage
}
