using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using Translaas.Models;
using Translaas.Models.Responses;

namespace Translaas.Samples.WebApi.Controllers;

/// <summary>
/// API controller providing direct access to Translaas SDK API methods.
/// This controller wraps the SDK API directly for testing and low-level access.
/// For real-world usage examples, see ProductsController, StatsController, and DashboardController.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TranslationController : ControllerBase
{
    private readonly ITranslaasService _translaasService;
    private readonly ITranslaasClient _translaasClient;
    private readonly ILogger<TranslationController> _logger;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationController"/> class.
    /// </summary>
    public TranslationController(
        ITranslaasService translaasService,
        ITranslaasClient translaasClient,
        ILogger<TranslationController> logger,
        IConfiguration configuration)
    {
        _translaasService = translaasService ?? throw new ArgumentNullException(nameof(translaasService));
        _translaasClient = translaasClient ?? throw new ArgumentNullException(nameof(translaasClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Gets a single translation entry using ITranslaasService.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">Optional language code (e.g., "en", "fr"). If omitted, language is resolved from HTTP request (query string, header, cookie) or thread culture.</param>
    /// <param name="number">Optional number for pluralization. Supports both integer and decimal/fractional numbers (e.g., 1.31).</param>
    /// <returns>The translated text.</returns>
    /// <remarks>
    /// Language resolution order:
    /// 1. Explicit lang parameter (if provided)
    /// 2. HTTP request sources (query string ?lang=en, header X-Language: en, cookie lang=en)
    /// 3. Thread culture (CultureInfo.CurrentUICulture)
    /// 4. Default language (from TranslaasOptions.DefaultLanguage)
    /// 
    /// Examples:
    /// - GET /api/translation/entry?group=common&entry=welcome&lang=en (explicit)
    /// - GET /api/translation/entry?group=common&entry=welcome (automatic from ?lang=en or header/cookie)
    /// </remarks>
    [HttpGet("entry")]
    public async Task<ActionResult<string>> GetEntry(
        [FromQuery] string group,
        [FromQuery] string entry,
        [FromQuery] string? lang = null,
        [FromQuery] decimal? number = null)
    {
        try
        {
            // If lang is not provided, it will use the default language from appsettings.json
            // via the language resolution providers (Request → Culture → Default)
            var defaultLanguage = _configuration["Translaas:DefaultLanguage"] ?? "en";
            string translation;
            if (number.HasValue)
            {
                if (lang != null)
                {
                    translation = await _translaasService.T(group, entry, lang, number.Value);
                }
                else
                {
                    translation = await _translaasService.T(group, entry, number.Value);
                }
            }
            else
            {
                if (lang != null)
                {
                    translation = await _translaasService.T(group, entry, lang);
                }
                else
                {
                    translation = await _translaasService.T(group, entry);
                }
            }
            return Ok(new 
            { 
                translation, 
                resolvedLanguage = lang ?? $"auto (default: {defaultLanguage})",
                defaultLanguage = defaultLanguage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving translation entry");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a single translation entry with named parameters using ITranslaasService.
    /// Named parameters are passed as query string parameters and can be used in translation placeholders like {userName}, {count}, etc.
    /// Example: GET /api/translation/entry-with-params?group=messages&entry=greeting&lang=en&userName=John&count=5
    /// Note: All query string parameters except 'group', 'entry', 'lang', and 'number' are treated as named parameters.
    /// </summary>
    /// <param name="group">The translation group name.</param>
    /// <param name="entry">The translation entry key.</param>
    /// <param name="lang">Optional language code (e.g., "en", "fr"). If omitted, language is resolved automatically.</param>
    /// <param name="number">Optional number for pluralization. Supports both integer and decimal/fractional numbers (e.g., 1.31).</param>
    /// <returns>The translated text.</returns>
    [HttpGet("entry-with-params")]
    public async Task<ActionResult<string>> GetEntryWithParams(
        [FromQuery] string group,
        [FromQuery] string entry,
        [FromQuery] string? lang = null,
        [FromQuery] decimal? number = null)
    {
        try
        {
            // Extract named parameters from query string (exclude known parameters)
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var knownParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "group", "entry", "lang", "number", "n" 
            };

            foreach (var kvp in Request.Query.Where(kvp => !knownParams.Contains(kvp.Key) && kvp.Value.Count > 0))
            {
                // Take the first value if multiple values are provided
                parameters[kvp.Key] = kvp.Value[0] ?? string.Empty;
            }

            string translation;
            if (parameters.Count > 0)
            {
                if (number.HasValue)
                {
                    if (lang != null)
                    {
                        translation = await _translaasService.T(group, entry, lang, number.Value, parameters);
                    }
                    else
                    {
                        translation = await _translaasService.T(group, entry, number.Value, parameters);
                    }
                }
                else
                {
                    if (lang != null)
                    {
                        translation = await _translaasService.T(group, entry, lang, parameters);
                    }
                    else
                    {
                        translation = await _translaasService.T(group, entry, parameters);
                    }
                }
            }
            else
            {
                if (number.HasValue)
                {
                    if (lang != null)
                    {
                        translation = await _translaasService.T(group, entry, lang, number.Value);
                    }
                    else
                    {
                        translation = await _translaasService.T(group, entry, number.Value);
                    }
                }
                else
                {
                    if (lang != null)
                    {
                        translation = await _translaasService.T(group, entry, lang);
                    }
                    else
                    {
                        translation = await _translaasService.T(group, entry);
                    }
                }
            }
            
            return Ok(new { translation, resolvedLanguage = lang ?? "auto", parameters });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving translation entry with parameters");
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
