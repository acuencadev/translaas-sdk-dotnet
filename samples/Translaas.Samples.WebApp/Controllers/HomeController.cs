using Microsoft.AspNetCore.Mvc;

using Translaas.Extensions.DependencyInjection;

namespace Translaas.Samples.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ITranslaasService _translaasService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        ITranslaasService translaasService,
        ILogger<HomeController> logger)
    {
        _translaasService = translaasService ?? throw new ArgumentNullException(nameof(translaasService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
