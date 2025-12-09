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
    /// Gets all translations for a translation group (bulk operation).
    /// Uses the default project "translaas-sdk-samples".
    /// Note: For single entries, prefer using the /entry endpoint with ITranslaasService.T().
    /// Use this endpoint when you need all entries in a group at once.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="lang">The language code (e.g., "en", "fr").</param>
    /// <param name="format">Optional format parameter.</param>
    /// <returns>A translation group containing all entries.</returns>
    [HttpGet("group")]
    public async Task<ActionResult<TranslationGroup>> GetGroup(
        [FromQuery] string group,
        [FromQuery] string lang,
        [FromQuery] string? format = null)
    {
        try
        {
            const string projectId = "translaas-sdk-samples";
            var translationGroup = await _translaasClient.GetGroupAsync(projectId, group, lang, format);
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
    /// Uses the default project "translaas-sdk-samples".
    /// </summary>
    /// <param name="lang">The language code (e.g., "en", "fr").</param>
    /// <param name="format">Optional format parameter.</param>
    /// <returns>A translation project containing all groups and entries.</returns>
    [HttpGet("project")]
    public async Task<ActionResult<TranslationProject>> GetProject(
        [FromQuery] string lang,
        [FromQuery] string? format = null)
    {
        try
        {
            const string projectId = "translaas-sdk-samples";
            var translationProject = await _translaasClient.GetProjectAsync(projectId, lang, format);
            return Ok(translationProject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving translation project");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets available locales for the default project "translaas-sdk-samples".
    /// </summary>
    /// <returns>Available locales for the project.</returns>
    [HttpGet("locales")]
    public async Task<ActionResult<ProjectLocales>> GetLocales()
    {
        try
        {
            const string projectId = "translaas-sdk-samples";
            var locales = await _translaasClient.GetProjectLocalesAsync(projectId);
            return Ok(locales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project locales");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
