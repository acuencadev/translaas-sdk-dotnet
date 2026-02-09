using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using Translaas.Models;
using Translaas.Models.Responses;
using L = Translaas.Models.LanguageCodes;

namespace Translaas.Samples.WebApi.Controllers;

/// <summary>
/// API controller demonstrating real-world usage of Translaas SDK for product data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ITranslaasService _translaasService;
    private readonly ITranslaasClient _translaasClient;
    private readonly ILogger<ProductsController> _logger;
    private readonly IConfiguration _configuration;

    private const string ProjectId = "translaas-sdk-samples";

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    public ProductsController(
        ITranslaasService translaasService,
        ITranslaasClient translaasClient,
        ILogger<ProductsController> logger,
        IConfiguration configuration)
    {
        _translaasService = translaasService ?? throw new ArgumentNullException(nameof(translaasService));
        _translaasClient = translaasClient ?? throw new ArgumentNullException(nameof(translaasClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Gets a list of products with translated names and descriptions.
    /// </summary>
    /// <param name="lang">Optional language code (e.g., "en", "fr"). If omitted, language is resolved automatically.</param>
    /// <returns>A list of products with translated information.</returns>
    [HttpGet]
    public async Task<ActionResult<ProductsResponse>> GetProducts([FromQuery] string? lang = null)
    {
        try
        {
            var defaultLanguage = _configuration["Translaas:DefaultLanguage"] ?? L.English;
            var resolvedLang = lang ?? defaultLanguage;

            // Mock product data - in a real app, this would come from a database
            var products = new List<ProductDto>
            {
                new ProductDto
                {
                    Id = 1,
                    Name = await _translaasService.T("common", "app.name", resolvedLang) + " - Product 1",
                    Description = await _translaasService.T("common", "welcome.message", resolvedLang),
                    Price = 1299.99m,
                    Category = await _translaasService.T("common", "app.name", resolvedLang),
                    InStock = true
                },
                new ProductDto
                {
                    Id = 2,
                    Name = await _translaasService.T("common", "app.name", resolvedLang) + " - Product 2",
                    Description = await _translaasService.T("common", "welcome.message", resolvedLang),
                    Price = 899.99m,
                    Category = await _translaasService.T("common", "app.name", resolvedLang),
                    InStock = true
                },
                new ProductDto
                {
                    Id = 3,
                    Name = await _translaasService.T("common", "app.name", resolvedLang) + " - Product 3",
                    Description = await _translaasService.T("common", "welcome.message", resolvedLang),
                    Price = 29.99m,
                    Category = await _translaasService.T("common", "app.name", resolvedLang),
                    InStock = false
                }
            };

            var totalCount = products.Count;
            var inStockCount = products.Count(p => p.InStock);

            var response = new ProductsResponse
            {
                Products = products,
                TotalCount = totalCount,
                InStockCount = inStockCount,
                Summary = await _translaasService.T("messages", "greeting", resolvedLang,
                    new Dictionary<string, string> { { "userName", await _translaasService.T("common", "app.name", resolvedLang) }, { "itemCount", totalCount.ToString() } }),
                Language = resolvedLang
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a single product by ID with translated information.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="lang">Optional language code (e.g., "en", "fr"). If omitted, language is resolved automatically.</param>
    /// <returns>The product with translated information.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id, [FromQuery] string? lang = null)
    {
        try
        {
            var defaultLanguage = _configuration["Translaas:DefaultLanguage"] ?? L.English;
            var resolvedLang = lang ?? defaultLanguage;

            // Mock product lookup - in a real app, this would come from a database
            ProductDto? product = id switch
            {
                1 => new ProductDto
                {
                    Id = 1,
                    Name = await _translaasService.T("common", "app.name", resolvedLang) + " - Product 1",
                    Description = await _translaasService.T("common", "welcome.message", resolvedLang),
                    Price = 1299.99m,
                    Category = await _translaasService.T("common", "app.name", resolvedLang),
                    InStock = true,
                    Details = await _translaasService.T("common", "welcome.message", resolvedLang)
                },
                2 => new ProductDto
                {
                    Id = 2,
                    Name = await _translaasService.T("common", "app.name", resolvedLang) + " - Product 2",
                    Description = await _translaasService.T("common", "welcome.message", resolvedLang),
                    Price = 899.99m,
                    Category = await _translaasService.T("common", "app.name", resolvedLang),
                    InStock = true,
                    Details = await _translaasService.T("common", "welcome.message", resolvedLang)
                },
                3 => new ProductDto
                {
                    Id = 3,
                    Name = await _translaasService.T("common", "app.name", resolvedLang) + " - Product 3",
                    Description = await _translaasService.T("common", "welcome.message", resolvedLang),
                    Price = 29.99m,
                    Category = await _translaasService.T("common", "app.name", resolvedLang),
                    InStock = false,
                    Details = await _translaasService.T("common", "welcome.message", resolvedLang)
                },
                _ => null
            };

            if (product == null)
            {
                var notFoundMessage = await _translaasService.T("common", "error", resolvedLang);
                return NotFound(new { error = notFoundMessage, productId = id });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets product categories with translated names.
    /// </summary>
    /// <param name="lang">Optional language code (e.g., "en", "fr"). If omitted, language is resolved automatically.</param>
    /// <returns>A list of product categories with translated names.</returns>
    [HttpGet("categories")]
    public async Task<ActionResult<CategoriesResponse>> GetCategories([FromQuery] string? lang = null)
    {
        try
        {
            var defaultLanguage = _configuration["Translaas:DefaultLanguage"] ?? L.English;
            var resolvedLang = lang ?? defaultLanguage;

            var categories = new List<CategoryDto>
            {
                new CategoryDto
                {
                    Id = "category1",
                    Name = await _translaasService.T("common", "app.name", resolvedLang) + " - Category 1",
                    Description = await _translaasService.T("common", "welcome.message", resolvedLang)
                },
                new CategoryDto
                {
                    Id = "category2",
                    Name = await _translaasService.T("common", "app.name", resolvedLang) + " - Category 2",
                    Description = await _translaasService.T("common", "welcome.message", resolvedLang)
                },
                new CategoryDto
                {
                    Id = "category3",
                    Name = await _translaasService.T("common", "app.name", resolvedLang) + " - Category 3",
                    Description = await _translaasService.T("common", "welcome.message", resolvedLang)
                }
            };

            var response = new CategoriesResponse
            {
                Categories = categories,
                Count = categories.Count,
                Language = resolvedLang
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

/// <summary>
/// Product data transfer object.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Gets or sets the product ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the product name (translated).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product description (translated).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the product category (translated).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the product is in stock.
    /// </summary>
    public bool InStock { get; set; }

    /// <summary>
    /// Gets or sets additional product details (translated).
    /// </summary>
    public string? Details { get; set; }
}

/// <summary>
/// Products response model.
/// </summary>
public class ProductsResponse
{
    /// <summary>
    /// Gets or sets the list of products.
    /// </summary>
    public List<ProductDto> Products { get; set; } = new();

    /// <summary>
    /// Gets or sets the total count of products.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the count of products in stock.
    /// </summary>
    public int InStockCount { get; set; }

    /// <summary>
    /// Gets or sets the summary message (translated).
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language used for translations.
    /// </summary>
    public string Language { get; set; } = string.Empty;
}

/// <summary>
/// Category data transfer object.
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Gets or sets the category ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category name (translated).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category description (translated).
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Categories response model.
/// </summary>
public class CategoriesResponse
{
    /// <summary>
    /// Gets or sets the list of categories.
    /// </summary>
    public List<CategoryDto> Categories { get; set; } = new();

    /// <summary>
    /// Gets or sets the count of categories.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the language used for translations.
    /// </summary>
    public string Language { get; set; } = string.Empty;
}
