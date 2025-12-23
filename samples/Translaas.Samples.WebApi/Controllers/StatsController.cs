using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Translaas.Extensions.DependencyInjection;
using L = Translaas.Models.LanguageCodes;

namespace Translaas.Samples.WebApi.Controllers;

/// <summary>
/// API controller demonstrating real-world usage of Translaas SDK for statistics and metrics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly ITranslaasService _translaasService;
    private readonly ILogger<StatsController> _logger;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatsController"/> class.
    /// </summary>
    public StatsController(
        ITranslaasService translaasService,
        ILogger<StatsController> logger,
        IConfiguration configuration)
    {
        _translaasService = translaasService ?? throw new ArgumentNullException(nameof(translaasService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Gets application statistics with translated labels and messages.
    /// </summary>
    /// <param name="lang">Optional language code (e.g., "en", "fr"). If omitted, language is resolved automatically.</param>
    /// <returns>Application statistics with translated labels.</returns>
    [HttpGet]
    public async Task<ActionResult<StatsResponse>> GetStats([FromQuery] string? lang = null)
    {
        try
        {
            var defaultLanguage = _configuration["Translaas:DefaultLanguage"] ?? L.English;
            var resolvedLang = lang ?? defaultLanguage;

            // Mock statistics - in a real app, this would come from a database or analytics service
            var totalUsers = 1250;
            var activeUsers = 342;
            var totalOrders = 5678;
            var pendingOrders = 23;
            var totalRevenue = 125000.50m;

            var stats = new StatsResponse
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                TotalRevenue = totalRevenue,
                Labels = new StatsLabels
                {
                    TotalUsers = await _translaasService.T("stats", "label.total.users", resolvedLang),
                    ActiveUsers = await _translaasService.T("stats", "label.active.users", resolvedLang),
                    TotalOrders = await _translaasService.T("stats", "label.total.orders", resolvedLang),
                    PendingOrders = await _translaasService.T("stats", "label.pending.orders", resolvedLang),
                    TotalRevenue = await _translaasService.T("stats", "label.total.revenue", resolvedLang)
                },
                Messages = new StatsMessages
                {
                    UsersOnline = await _translaasService.T("messages", "user.online", resolvedLang, activeUsers),
                    ItemsInStock = await _translaasService.T("messages", "item", resolvedLang, pendingOrders),
                    Summary = await _translaasService.T("stats", "summary", resolvedLang, null,
                        new Dictionary<string, string>
                        {
                            { "totalUsers", totalUsers.ToString() },
                            { "activeUsers", activeUsers.ToString() },
                            { "totalOrders", totalOrders.ToString() }
                        })
                },
                Language = resolvedLang
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets user statistics with translated messages using pluralization.
    /// </summary>
    /// <param name="lang">Optional language code (e.g., "en", "fr"). If omitted, language is resolved automatically.</param>
    /// <returns>User statistics with translated messages.</returns>
    [HttpGet("users")]
    public async Task<ActionResult<UserStatsResponse>> GetUserStats([FromQuery] string? lang = null)
    {
        try
        {
            var defaultLanguage = _configuration["Translaas:DefaultLanguage"] ?? L.English;
            var resolvedLang = lang ?? defaultLanguage;

            // Mock user statistics
            var totalUsers = 1250;
            var onlineUsers = 342;
            var newUsersToday = 5;
            var newUsersThisWeek = 23;

            var response = new UserStatsResponse
            {
                TotalUsers = totalUsers,
                OnlineUsers = onlineUsers,
                NewUsersToday = newUsersToday,
                NewUsersThisWeek = newUsersThisWeek,
                Messages = new UserStatsMessages
                {
                    TotalUsersMessage = await _translaasService.T("stats", "users.total", resolvedLang, totalUsers),
                    OnlineUsersMessage = await _translaasService.T("messages", "user.online", resolvedLang, onlineUsers),
                    NewUsersTodayMessage = await _translaasService.T("stats", "users.new.today", resolvedLang, newUsersToday),
                    NewUsersThisWeekMessage = await _translaasService.T("stats", "users.new.week", resolvedLang, newUsersThisWeek)
                },
                Language = resolvedLang
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user statistics");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets order statistics with translated messages.
    /// </summary>
    /// <param name="lang">Optional language code (e.g., "en", "fr"). If omitted, language is resolved automatically.</param>
    /// <returns>Order statistics with translated messages.</returns>
    [HttpGet("orders")]
    public async Task<ActionResult<OrderStatsResponse>> GetOrderStats([FromQuery] string? lang = null)
    {
        try
        {
            var defaultLanguage = _configuration["Translaas:DefaultLanguage"] ?? L.English;
            var resolvedLang = lang ?? defaultLanguage;

            // Mock order statistics
            var totalOrders = 5678;
            var pendingOrders = 23;
            var completedOrders = 5600;
            var cancelledOrders = 55;

            var response = new OrderStatsResponse
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                Messages = new OrderStatsMessages
                {
                    TotalOrdersMessage = await _translaasService.T("stats", "orders.total", resolvedLang, totalOrders),
                    PendingOrdersMessage = await _translaasService.T("stats", "orders.pending", resolvedLang, pendingOrders),
                    CompletedOrdersMessage = await _translaasService.T("stats", "orders.completed", resolvedLang, completedOrders),
                    CancelledOrdersMessage = await _translaasService.T("stats", "orders.cancelled", resolvedLang, cancelledOrders)
                },
                Language = resolvedLang
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order statistics");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

/// <summary>
/// Statistics response model.
/// </summary>
public class StatsResponse
{
    /// <summary>
    /// Gets or sets the total number of users.
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of active users.
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Gets or sets the total number of orders.
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Gets or sets the number of pending orders.
    /// </summary>
    public int PendingOrders { get; set; }

    /// <summary>
    /// Gets or sets the total revenue.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Gets or sets the translated labels.
    /// </summary>
    public StatsLabels Labels { get; set; } = new();

    /// <summary>
    /// Gets or sets the translated messages.
    /// </summary>
    public StatsMessages Messages { get; set; } = new();

    /// <summary>
    /// Gets or sets the language used for translations.
    /// </summary>
    public string Language { get; set; } = string.Empty;
}

/// <summary>
/// Statistics labels model.
/// </summary>
public class StatsLabels
{
    /// <summary>
    /// Gets or sets the "Total Users" label (translated).
    /// </summary>
    public string TotalUsers { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "Active Users" label (translated).
    /// </summary>
    public string ActiveUsers { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "Total Orders" label (translated).
    /// </summary>
    public string TotalOrders { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "Pending Orders" label (translated).
    /// </summary>
    public string PendingOrders { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "Total Revenue" label (translated).
    /// </summary>
    public string TotalRevenue { get; set; } = string.Empty;
}

/// <summary>
/// Statistics messages model.
/// </summary>
public class StatsMessages
{
    /// <summary>
    /// Gets or sets the "Users Online" message (translated).
    /// </summary>
    public string UsersOnline { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "Items In Stock" message (translated).
    /// </summary>
    public string ItemsInStock { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the summary message (translated).
    /// </summary>
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// User statistics response model.
/// </summary>
public class UserStatsResponse
{
    /// <summary>
    /// Gets or sets the total number of users.
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of online users.
    /// </summary>
    public int OnlineUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of new users today.
    /// </summary>
    public int NewUsersToday { get; set; }

    /// <summary>
    /// Gets or sets the number of new users this week.
    /// </summary>
    public int NewUsersThisWeek { get; set; }

    /// <summary>
    /// Gets or sets the translated messages.
    /// </summary>
    public UserStatsMessages Messages { get; set; } = new();

    /// <summary>
    /// Gets or sets the language used for translations.
    /// </summary>
    public string Language { get; set; } = string.Empty;
}

/// <summary>
/// User statistics messages model.
/// </summary>
public class UserStatsMessages
{
    /// <summary>
    /// Gets or sets the total users message (translated).
    /// </summary>
    public string TotalUsersMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the online users message (translated).
    /// </summary>
    public string OnlineUsersMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new users today message (translated).
    /// </summary>
    public string NewUsersTodayMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new users this week message (translated).
    /// </summary>
    public string NewUsersThisWeekMessage { get; set; } = string.Empty;
}

/// <summary>
/// Order statistics response model.
/// </summary>
public class OrderStatsResponse
{
    /// <summary>
    /// Gets or sets the total number of orders.
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Gets or sets the number of pending orders.
    /// </summary>
    public int PendingOrders { get; set; }

    /// <summary>
    /// Gets or sets the number of completed orders.
    /// </summary>
    public int CompletedOrders { get; set; }

    /// <summary>
    /// Gets or sets the number of cancelled orders.
    /// </summary>
    public int CancelledOrders { get; set; }

    /// <summary>
    /// Gets or sets the translated messages.
    /// </summary>
    public OrderStatsMessages Messages { get; set; } = new();

    /// <summary>
    /// Gets or sets the language used for translations.
    /// </summary>
    public string Language { get; set; } = string.Empty;
}

/// <summary>
/// Order statistics messages model.
/// </summary>
public class OrderStatsMessages
{
    /// <summary>
    /// Gets or sets the total orders message (translated).
    /// </summary>
    public string TotalOrdersMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pending orders message (translated).
    /// </summary>
    public string PendingOrdersMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the completed orders message (translated).
    /// </summary>
    public string CompletedOrdersMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cancelled orders message (translated).
    /// </summary>
    public string CancelledOrdersMessage { get; set; } = string.Empty;
}
