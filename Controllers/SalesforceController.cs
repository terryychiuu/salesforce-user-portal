using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SalesforceManager.Models;
using SalesforceManager.Services.Salesforce;
using SalesforceManager.Services.Salesforce.Configuration;

namespace SalesforceManager.Controllers
{
    [ApiController]
    [Route("api/sf")]
    public class SalesforceController : ControllerBase
    {
        private readonly ILogger<SalesforceController> _logger;
        private readonly SalesforceService _salesforceService;
        private readonly HashSet<string> _adminUsernames;

        public SalesforceController(
            ILogger<SalesforceController> logger,
            SalesforceService salesforceService,
            IOptions<SalesforceConfig> salesforceOptions)
        {
            _logger = logger;
            _salesforceService = salesforceService;
            _adminUsernames = new HashSet<string>(salesforceOptions.Value.AdminUsernames ?? []);
        }

        [HttpGet("users")]
        public async Task<IActionResult> Users(
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] string? search,
            [FromQuery] List<string>? roleId,
            [FromQuery] string? status,
            [FromQuery] string? lastLogin,
            CancellationToken cancellationToken)
        {
            try
            {
                var users = await _salesforceService.GetUsers(
                    sortBy,
                    sortDirection,
                    search,
                    roleId,
                    status,
                    lastLogin,
                    cancellationToken
                );
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetch users error.");
                return Problem(
                    title: "Fetch users error.",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> Roles(CancellationToken cancellationToken)
        {
            try
            {
                var roles = await _salesforceService.GetRoles(cancellationToken);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetch roles error.");
                return Problem(
                    title: "Fetch roles error.",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch("users/{userId}/active")]
        public async Task<IActionResult> UpdateUserActiveStatus(
            [FromRoute] string userId,
            [FromBody] SalesforceUserActiveUpdateRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("User id is required.");

            if (request?.Inactive is null)
                return BadRequest("Body must include 'inactive'.");

            try
            {
                var user = await _salesforceService.GetUserById(userId, cancellationToken);
                if (user is null)
                    return NotFound("User was not found.");

                if (_adminUsernames.Contains(user.Username))
                {
                    return Problem(
                        title: "Activation update blocked.",
                        detail: "Activation cannot be changed for this user.",
                        statusCode: StatusCodes.Status403Forbidden);
                }

                await _salesforceService.UpdateUserActiveStatus(
                    userId,
                    isActive: !request.Inactive.Value,
                    cancellationToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update active error.");
                return Problem(
                    title: "Update active error.",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
