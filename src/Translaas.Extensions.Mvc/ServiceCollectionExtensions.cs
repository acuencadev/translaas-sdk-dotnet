using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.DependencyInjection;

namespace Translaas.Extensions.Mvc;

/// <summary>
/// Extension methods for configuring Translaas MVC/Razor services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Translaas MVC/Razor services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when services is null.</exception>
    /// <remarks>
    /// <para>
    /// This method registers Translaas Tag Helpers for use in Razor views.
    /// </para>
    /// <para>
    /// After calling this method, you can use the following in your Razor views:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>Tag Helper: <c>&lt;translaas group="..." entry="..." lang="..." /&gt;</c></description>
    /// </item>
    /// <item>
    /// <description>Static Helper: <c>@Translaas.T(Html, "group", "entry", "lang")</c></description>
    /// </item>
    /// <item>
    /// <description>Direct Service: <c>@inject ITranslaasService Translaas</c> then <c>@await Translaas.T("group", "entry", "lang")</c></description>
    /// </item>
    /// </list>
    /// <para>
    /// Make sure to add the following to your <c>_ViewImports.cshtml</c>:
    /// </para>
    /// <code>
    /// @addTagHelper *, Translaas.Extensions.Mvc
    /// @using Translaas.Extensions.Mvc
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddTranslaas(options => { /* ... */ });
    /// services.AddTranslaasMvc();
    /// </code>
    /// </example>
    public static IServiceCollection AddTranslaasMvc(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new System.ArgumentNullException(nameof(services));
        }

        // Tag helpers are automatically discovered, but we can add explicit registration if needed
        // The main requirement is that ITranslaasService is registered (via AddTranslaas)

        return services;
    }
}
