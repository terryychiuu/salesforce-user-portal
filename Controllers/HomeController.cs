using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SalesforceManager.Models;
using SalesforceManager.Services.Salesforce;
using SalesforceManager.Services.Salesforce.Configuration;
using System.Diagnostics;

namespace SalesforceManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SalesforceService _salesforceService;
        private readonly SalesforceConfig _salesforceConfig;

        public HomeController(
            ILogger<HomeController> logger,
            SalesforceService salesforceService,
            IOptions<SalesforceConfig> salesforceOptions)
        {
            _logger = logger;
            _salesforceService = salesforceService;
            _salesforceConfig = salesforceOptions.Value;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var usersTask = _salesforceService.GetUsers();
                var rolesTask = _salesforceService.GetRoles();
                var activeUsersCountTask = _salesforceService.GetActiveUsersCount();

                await Task.WhenAll(usersTask, rolesTask, activeUsersCountTask);

                return View(new HomeViewModel
                {
                    Users = usersTask.Result,
                    Roles = rolesTask.Result,
                    ActiveUsersTotal = activeUsersCountTask.Result,
                    AdminUsernames = _salesforceConfig.AdminUsernames ?? []
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
