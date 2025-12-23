using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using L = Translaas.Models.LanguageCodes;

namespace Translaas.Samples.WebApi.Controllers;

/// <summary>
/// API controller demonstrating real-world usage of Translaas SDK for dashboard data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ITranslaasService _translaasService;
    private readonly ITranslaasClient _translaasClient;
    private readonly ILogger<DashboardController> _logger;
    private readonly IConfiguration _configuration;

    private const string ProjectId = "translaas-sdk-samples";

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardController"/> class.
    /// </summary>
    public DashboardController(
        ITranslaasService translaasService,
        ITranslaasClient translaasClient,
        ILogger<DashboardController> logger,
        IConfiguration configuration)
    {
        _translaasService = translaasService ?? throw new ArgumentNullException(nameof(translaasService));
        _translaasClient = translaasClient ?? throw new ArgumentNullException(nameof(translaasClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Gets dashboard data with translated labels, messages, and metrics.
    /// </summary>
    /// <param name="lang">Optional language code (e.g., "en", "fr"). If omitted, language is resolved automatically.</param>
    /// <returns>Dashboard data with all translated content.</returns>
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> GetDashboard([FromQuery] string? lang = null)
    {
        try
        {
            var defaultLanguage = _configuration["Translaas:DefaultLanguage"] ?? L.English;
            var resolvedLang = lang ?? defaultLanguage;

            // Load translation groups for dashboard
            var commonGroup = await _translaasClient.GetGroupAsync(ProjectId, "common", resolvedLang);
            var messagesGroup = await _translaasClient.GetGroupAsync(ProjectId, "messages", resolvedLang);

            // Mock dashboard data - in a real app, this would come from various data sources
            var totalUsers = 1250;
            var activeUsers = 342;
            var totalOrders = 5678;
            var pendingOrders = 23;
            var totalRevenue = 125000.50m;
            var notifications = 5;

            var dashboard = new DashboardResponse
            {
                Title = await _translaasService.T("common", "welcome", resolvedLang),
                Subtitle = await _translaasService.T("dashboard", "subtitle", resolvedLang),
                Metrics = new List<MetricDto>
                {
                    new MetricDto
                    {
                        Label = await _translaasService.T("stats", "label.total.users", resolvedLang),
                        Value = totalUsers,
                        FormattedValue = totalUsers.ToString("N0"),
                        Message = await _translaasService.T("stats", "users.total", resolvedLang, totalUsers)
                    },
                    new MetricDto
                    {
                        Label = await _translaasService.T("stats", "label.active.users", resolvedLang),
                        Value = activeUsers,
                        FormattedValue = activeUsers.ToString("N0"),
                        Message = await _translaasService.T("messages", "user.online", resolvedLang, activeUsers)
                    },
                    new MetricDto
                    {
                        Label = await _translaasService.T("stats", "label.total.orders", resolvedLang),
                        Value = totalOrders,
                        FormattedValue = totalOrders.ToString("N0"),
                        Message = await _translaasService.T("stats", "orders.total", resolvedLang, totalOrders)
                    },
                    new MetricDto
                    {
                        Label = await _translaasService.T("stats", "label.pending.orders", resolvedLang),
                        Value = pendingOrders,
                        FormattedValue = pendingOrders.ToString("N0"),
                        Message = await _translaasService.T("stats", "orders.pending", resolvedLang, pendingOrders)
                    },
                    new MetricDto
                    {
                        Label = await _translaasService.T("stats", "label.total.revenue", resolvedLang),
                        Value = (double)totalRevenue,
                        FormattedValue = totalRevenue.ToString("C"),
                        Message = await _translaasService.T("stats", "revenue.total", resolvedLang,
                            new Dictionary<string, string> { { "amount", totalRevenue.ToString("C") } })
                    }
                },
                Notifications = new NotificationDto
                {
                    Count = notifications,
                    Message = await _translaasService.T("messages", "notification", resolvedLang, notifications),
                    Label = await _translaasService.T("dashboard", "notifications.label", resolvedLang)
                },
                RecentActivity = new List<ActivityDto>
                {
                    new ActivityDto
                    {
                        Type = await _translaasService.T("dashboard", "activity.type.order", resolvedLang),
                        Description = await _translaasService.T("dashboard", "activity.order.created", resolvedLang,
                            new Dictionary<string, string> { { "orderId", "12345" } }),
                        Timestamp = DateTime.UtcNow.AddMinutes(-5)
                    },
                    new ActivityDto
                    {
                        Type = await _translaasService.T("dashboard", "activity.type.user", resolvedLang),
                        Description = await _translaasService.T("dashboard", "activity.user.registered", resolvedLang,
                            new Dictionary<string, string> { { "userName", "John Doe" } }),
                        Timestamp = DateTime.UtcNow.AddMinutes(-15)
                    },
                    new ActivityDto
                    {
                        Type = await _translaasService.T("dashboard", "activity.type.product", resolvedLang),
                        Description = await _translaasService.T("dashboard", "activity.product.updated", resolvedLang,
                            new Dictionary<string, string> { { "productName", await _translaasService.T("products", "laptop.name", resolvedLang) } }),
                        Timestamp = DateTime.UtcNow.AddHours(-1)
                    }
                },
                Summary = await _translaasService.T("dashboard", "summary", resolvedLang,
                    new Dictionary<string, string>
                    {
                        { "totalUsers", totalUsers.ToString() },
                        { "activeUsers", activeUsers.ToString() },
                        { "totalOrders", totalOrders.ToString() },
                        { "totalRevenue", totalRevenue.ToString("C") }
                    }),
                Language = resolvedLang
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets dashboard summary with key metrics and translated messages.
    /// </summary>
    /// <param name="lang">Optional language code (e.g., "en", "fr"). If omitted, language is resolved automatically.</param>
    /// <returns>Dashboard summary with translated content.</returns>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary([FromQuery] string? lang = null)
    {
        try
        {
            var defaultLanguage = _configuration["Translaas:DefaultLanguage"] ?? L.English;
            var resolvedLang = lang ?? defaultLanguage;

            // Mock summary data
            var totalUsers = 1250;
            var activeUsers = 342;
            var totalOrders = 5678;
            var totalRevenue = 125000.50m;

            var summary = new DashboardSummaryResponse
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                Messages = new DashboardSummaryMessages
                {
                    WelcomeMessage = await _translaasService.T("common", "welcome", resolvedLang),
                    UsersMessage = await _translaasService.T("messages", "user.online", resolvedLang, activeUsers),
                    OrdersMessage = await _translaasService.T("stats", "orders.total", resolvedLang, totalOrders),
                    RevenueMessage = await _translaasService.T("stats", "revenue.total", resolvedLang,
                        new Dictionary<string, string> { { "amount", totalRevenue.ToString("C") } })
                },
                Language = resolvedLang
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

/// <summary>
/// Dashboard response model.
/// </summary>
public class DashboardResponse
{
    /// <summary>
    /// Gets or sets the dashboard title (translated).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dashboard subtitle (translated).
    /// </summary>
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of metrics.
    /// </summary>
    public List<MetricDto> Metrics { get; set; } = new();

    /// <summary>
    /// Gets or sets the notifications information.
    /// </summary>
    public NotificationDto Notifications { get; set; } = new();

    /// <summary>
    /// Gets or sets the recent activity list.
    /// </summary>
    public List<ActivityDto> RecentActivity { get; set; } = new();

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
/// Metric data transfer object.
/// </summary>
public class MetricDto
{
    /// <summary>
    /// Gets or sets the metric label (translated).
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metric value.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the formatted value string.
    /// </summary>
    public string FormattedValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metric message (translated).
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Notification data transfer object.
/// </summary>
public class NotificationDto
{
    /// <summary>
    /// Gets or sets the notification count.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the notification message (translated).
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification label (translated).
    /// </summary>
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// Activity data transfer object.
/// </summary>
public class ActivityDto
{
    /// <summary>
    /// Gets or sets the activity type (translated).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the activity description (translated).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the activity timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Dashboard summary response model.
/// </summary>
public class DashboardSummaryResponse
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
    /// Gets or sets the total revenue.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Gets or sets the translated messages.
    /// </summary>
    public DashboardSummaryMessages Messages { get; set; } = new();

    /// <summary>
    /// Gets or sets the language used for translations.
    /// </summary>
    public string Language { get; set; } = string.Empty;
}

/// <summary>
/// Dashboard summary messages model.
/// </summary>
public class DashboardSummaryMessages
{
    /// <summary>
    /// Gets or sets the welcome message (translated).
    /// </summary>
    public string WelcomeMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the users message (translated).
    /// </summary>
    public string UsersMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the orders message (translated).
    /// </summary>
    public string OrdersMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the revenue message (translated).
    /// </summary>
    public string RevenueMessage { get; set; } = string.Empty;
}
