using Microsoft.AspNetCore.Mvc;
using SalesforceManager.Models;
using SalesforceManager.Services.Salesforce;
using System.Diagnostics;

namespace SalesforceManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SalesforceService _salesforceService;

        public HomeController(ILogger<HomeController> logger, SalesforceService salesforceService)
        {
            _logger = logger;
            _salesforceService = salesforceService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _salesforceService.GetUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetch users error.");
                ViewBag.ErrorMessage = $"Fetch users error. {ex.Message}";
                return View(Array.Empty<SalesforceUserDto>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
