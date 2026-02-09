using System.Threading.Tasks;

using Microsoft.AspNetCore.Razor.TagHelpers;

using Translaas.Extensions.DependencyInjection;

namespace Translaas.Extensions.Mvc;

/// <summary>
/// Tag helper for rendering translations in Razor views.
/// </summary>
/// <remarks>
/// <para>
/// Use this tag helper to render translations directly in Razor views.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// &lt;!-- With explicit language --&gt;
/// &lt;translaas group="common" entry="welcome" lang="en" /&gt;
/// 
/// &lt;!-- With language resolution (requires providers configured) --&gt;
/// &lt;translaas group="common" entry="welcome" /&gt;
/// 
/// &lt;translaas group="messages" entry="item" lang="en" number="5" /&gt;
/// </code>
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="TranslaasTagHelper"/> class.
/// </remarks>
/// <param name="translaasService">The Translaas service for translation lookups.</param>
/// <exception cref="System.ArgumentNullException">Thrown when translaasService is null.</exception>
[HtmlTargetElement("translaas")]
public class TranslaasTagHelper(ITranslaasService translaasService) : TagHelper
{
    private readonly ITranslaasService _translaasService = translaasService ?? throw new System.ArgumentNullException(nameof(translaasService));

    /// <summary>
    /// Gets or sets the translation group name.
    /// </summary>
    [HtmlAttributeName("group")]
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the translation entry key.
    /// </summary>
    [HtmlAttributeName("entry")]
    public string Entry { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language code (e.g., "en", "fr").
    /// Optional when language providers are configured.
    /// </summary>
    [HtmlAttributeName("lang")]
    public string? Lang { get; set; }

    /// <summary>
    /// Gets or sets the optional number for pluralization. Supports both integer and decimal/fractional numbers (e.g., 1.31).
    /// </summary>
    [HtmlAttributeName("number")]
    public decimal? Number { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Group))
        {
            throw new System.ArgumentException("Group is required.", nameof(Group));
        }

        if (string.IsNullOrWhiteSpace(Entry))
        {
            throw new System.ArgumentException("Entry is required.", nameof(Entry));
        }

        if (output == null)
        {
            throw new System.ArgumentNullException(nameof(output));
        }

        // Suppress the original tag
        output.TagName = null;

        // Get the translation using appropriate overload based on provided parameters
        var translationTask = !string.IsNullOrWhiteSpace(Lang)
            ? (Number.HasValue
                ? _translaasService.T(Group, Entry, Lang, Number.Value)
                : _translaasService.T(Group, Entry, Lang))
            : (Number.HasValue
                ? _translaasService.T(Group, Entry, Number.Value)
                : _translaasService.T(Group, Entry));

        var translation = await translationTask.ConfigureAwait(false);

        // Set the output content
        output.Content.SetHtmlContent(translation);
    }
}
