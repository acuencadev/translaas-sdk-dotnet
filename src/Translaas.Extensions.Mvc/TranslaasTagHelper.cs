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
/// &lt;translaas group="common" entry="welcome" lang="en" /&gt;
/// &lt;translaas group="messages" entry="item" lang="en" number="5" /&gt;
/// </code>
/// </para>
/// </remarks>
[HtmlTargetElement("translaas")]
public class TranslaasTagHelper : TagHelper
{
    private readonly ITranslaasService _translaasService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasTagHelper"/> class.
    /// </summary>
    /// <param name="translaasService">The Translaas service for translation lookups.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when translaasService is null.</exception>
    public TranslaasTagHelper(ITranslaasService translaasService)
    {
        _translaasService = translaasService ?? throw new System.ArgumentNullException(nameof(translaasService));
    }

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
    /// </summary>
    [HtmlAttributeName("lang")]
    public string Lang { get; set; } = string.Empty;

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

        if (string.IsNullOrWhiteSpace(Lang))
        {
            throw new System.ArgumentException("Lang is required.", nameof(Lang));
        }

        if (output == null)
        {
            throw new System.ArgumentNullException(nameof(output));
        }

        // Suppress the original tag
        output.TagName = null;

        // Get the translation
        var translation = await _translaasService.T(Group, Entry, Lang, Number).ConfigureAwait(false);

        // Set the output content
        output.Content.SetHtmlContent(translation);
    }
}
