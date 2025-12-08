using Microsoft.AspNetCore.Mvc;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using Translaas.Models.Responses;

namespace Translaas.Samples.WebApi.Controllers;

/// <summary>
/// API controller demonstrating Translaas SDK usage in ASP.NET Core Web API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TranslationController : ControllerBase
{
    private readonly ITranslaasService _translaasService;
    private readonly ITranslaasClient _translaasClient;
    private readonly ILogger<TranslationController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationController"/> class.
    /// </summary>
    public TranslationController(
        ITranslaasService translaasService,
        ITranslaasClient translaasClient,
        ILogger<TranslationController> logger)
    {
        _translaasService = translaasService ?? throw new ArgumentNullException(nameof(translaasService));
        _translaasClient = translaasClient ?? throw new ArgumentNullException(nameof(translaasClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a single translation entry using ITranslaasService.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code (e.g., "en", "fr").</param>
    /// <param name="number">Optional number for pluralization.</param>
    /// <returns>The translated text.</returns>
    [HttpGet("entry")]
    public async Task<ActionResult<string>> GetEntry(
        [FromQuery] string group,
        [FromQuery] string entry,
        [FromQuery] string lang,
        [FromQuery] int? number = null)
    {
        try
        {
            var translation = await _translaasService.T(group, entry, lang, number);
            return Ok(translation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving translation entry");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a single translation entry using ITranslaasClient.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">The language code (e.g., "en", "fr").</param>
    /// <param name="number">Optional number for pluralization.</param>
    /// <returns>The translated text.</returns>
    [HttpGet("entry/client")]
    public async Task<ActionResult<string>> GetEntryUsingClient(
        [FromQuery] string group,
        [FromQuery] string entry,
        [FromQuery] string lang,
        [FromQuery] int? number = null)
    {
        try
        {
            var translation = await _translaasClient.GetEntryAsync(group, entry, lang, number);
            return Ok(translation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving translation entry");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all translations for a translation group.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="group">The translation group name.</param>
    /// <param name="lang">The language code (e.g., "en", "fr").</param>
    /// <param name="format">Optional format parameter.</param>
    /// <returns>A translation group containing all entries.</returns>
    [HttpGet("group")]
    public async Task<ActionResult<TranslationGroup>> GetGroup(
        [FromQuery] string project,
        [FromQuery] string group,
        [FromQuery] string lang,
        [FromQuery] string? format = null)
    {
        try
        {
            var translationGroup = await _translaasClient.GetGroupAsync(project, group, lang, format);
            return Ok(translationGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving translation group");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all translations for a project.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="lang">The language code (e.g., "en", "fr").</param>
    /// <param name="format">Optional format parameter.</param>
    /// <returns>A translation project containing all groups and entries.</returns>
    [HttpGet("project")]
    public async Task<ActionResult<TranslationProject>> GetProject(
        [FromQuery] string project,
        [FromQuery] string lang,
        [FromQuery] string? format = null)
    {
        try
        {
            var translationProject = await _translaasClient.GetProjectAsync(project, lang, format);
            return Ok(translationProject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving translation project");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets available locales for a project.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <returns>Available locales for the project.</returns>
    [HttpGet("locales")]
    public async Task<ActionResult<ProjectLocales>> GetLocales([FromQuery] string project)
    {
        try
        {
            var locales = await _translaasClient.GetProjectLocalesAsync(project);
            return Ok(locales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project locales");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
