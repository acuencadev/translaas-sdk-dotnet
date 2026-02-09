using Microsoft.AspNetCore.Mvc;

using Translaas.Extensions.DependencyInjection;

namespace Translaas.Samples.WebApp.Controllers;

public class HomeController(
    ITranslaasService translaasService,
    ILogger<HomeController> logger) : Controller
{
    private readonly ITranslaasService _translaasService = translaasService ?? throw new ArgumentNullException(nameof(translaasService));
    private readonly ILogger<HomeController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
