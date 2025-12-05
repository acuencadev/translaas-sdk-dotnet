using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

using Translaas.Extensions.DependencyInjection;

namespace Translaas.Extensions.Mvc;

/// <summary>
/// Static helper class for rendering translations in Razor views.
/// </summary>
/// <remarks>
/// <para>
/// Provides a convenient static API for translation lookups in Razor views.
/// This is the preferred approach for consistency with the Tag Helper name and service name.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// @Translaas.T(Html, "common", "welcome", "en")
/// @Translaas.T(Html, "messages", "item", "en", 5)
/// </code>
/// </para>
/// <para>
/// Note: <c>Html</c> is available by default in Razor views (no injection needed).
/// </para>
/// <para>
/// For async support, inject ITranslaasService directly:
/// <code>
/// @inject ITranslaasService Translaas
/// @await Translaas.T("common", "welcome", "en")
/// </code>
/// </para>
/// </remarks>
public static class Translaas
{
    /// <summary>
    /// Gets a translation entry and renders it as HTML.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper instance (available as <c>Html</c> in Razor views by default).</param>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code (e.g., "en", "fr").</param>
    /// <param name="number">Optional number for pluralization.</param>
    /// <returns>An HTML-encoded string containing the translation.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when htmlHelper is null.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when ITranslaasService is not registered.</exception>
    /// <example>
    /// <code>
    /// @Translaas.T(Html, "common", "welcome", "en")
    /// @Translaas.T(Html, "messages", "item", "en", 5)
    /// </code>
    /// </example>
    public static IHtmlContent T(
        IHtmlHelper htmlHelper,
        string group,
        string entry,
        string lang,
        int? number = null)
    {
        if (htmlHelper == null)
        {
            throw new System.ArgumentNullException(nameof(htmlHelper));
        }

        var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
#if NETSTANDARD2_0
        var translaasService = (ITranslaasService?)serviceProvider.GetService(typeof(ITranslaasService));
#else
        var translaasService = serviceProvider.GetService<ITranslaasService>();
#endif

        if (translaasService == null)
        {
            throw new System.InvalidOperationException(
                "ITranslaasService is not registered in the service collection. " +
                "Ensure you have called services.AddTranslaas() in your startup configuration.");
        }

        // For synchronous helpers, we need to get the result synchronously
        var translation = translaasService.T(group, entry, lang, number).GetAwaiter().GetResult();

        return new HtmlString(translation);
    }
}
