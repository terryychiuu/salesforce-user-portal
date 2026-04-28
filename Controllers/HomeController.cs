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
                var usersTask = _salesforceService.GetUsersAsync();
                var rolesTask = _salesforceService.GetRolesAsync();

                await Task.WhenAll(usersTask, rolesTask);

                return View(new HomeViewModel
                {
                    Users = usersTask.Result,
                    Roles = rolesTask.Result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetch users error.");
                ViewBag.ErrorMessage = $"Fetch users error. {ex.Message}";
                return View(new HomeViewModel());
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
