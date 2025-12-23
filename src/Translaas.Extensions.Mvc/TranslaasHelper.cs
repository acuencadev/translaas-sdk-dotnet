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
    /// Gets a translation entry and renders it as HTML using automatic language resolution.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper instance (available as <c>Html</c> in Razor views by default).</param>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <returns>An HTML-encoded string containing the translation.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when htmlHelper is null.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when ITranslaasService is not registered or no language can be resolved.</exception>
    public static IHtmlContent T(IHtmlHelper htmlHelper, string group, string entry)
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

        var translation = translaasService.T(group, entry).ConfigureAwait(false).GetAwaiter().GetResult();
        return new HtmlString(translation);
    }

    /// <summary>
    /// Gets a translation entry and renders it as HTML with explicit language override.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper instance (available as <c>Html</c> in Razor views by default).</param>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code (e.g., "en", "fr"). Bypasses all language providers. Can be null to use language resolution.</param>
    /// <returns>An HTML-encoded string containing the translation.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when htmlHelper is null.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when ITranslaasService is not registered.</exception>
    public static IHtmlContent T(IHtmlHelper htmlHelper, string group, string entry, string? lang)
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

        var translation = translaasService.T(group, entry, lang).ConfigureAwait(false).GetAwaiter().GetResult();
        return new HtmlString(translation);
    }

    /// <summary>
    /// Gets a translation entry and renders it as HTML with explicit language and pluralization number.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper instance (available as <c>Html</c> in Razor views by default).</param>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code (e.g., "en", "fr"). Bypasses all language providers. Can be null to use language resolution.</param>
    /// <param name="number">Number for pluralization. Supports both integer and decimal/fractional numbers (e.g., 1.31).</param>
    /// <returns>An HTML-encoded string containing the translation.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when htmlHelper is null.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when ITranslaasService is not registered.</exception>
    public static IHtmlContent T(IHtmlHelper htmlHelper, string group, string entry, string? lang, decimal number)
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

        var translation = translaasService.T(group, entry, lang, number).ConfigureAwait(false).GetAwaiter().GetResult();
        return new HtmlString(translation);
    }

    /// <summary>
    /// Gets a translation entry and renders it as HTML using automatic language resolution with pluralization number.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper instance (available as <c>Html</c> in Razor views by default).</param>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="number">Number for pluralization. Supports both integer and decimal/fractional numbers (e.g., 1.31).</param>
    /// <returns>An HTML-encoded string containing the translation.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when htmlHelper is null.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when ITranslaasService is not registered or no language can be resolved.</exception>
    public static IHtmlContent T(IHtmlHelper htmlHelper, string group, string entry, decimal number)
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

        var translation = translaasService.T(group, entry, number).ConfigureAwait(false).GetAwaiter().GetResult();
        return new HtmlString(translation);
    }
}
