using Microsoft.AspNetCore.Mvc;
using SalesforceManager.Models;
using SalesforceManager.Services.Salesforce;

namespace SalesforceManager.Controllers
{
    [ApiController]
    [Route("api/sf")]
    public class SalesforceController : ControllerBase
    {
        private readonly ILogger<SalesforceController> _logger;
        private readonly SalesforceService _salesforceService;

        public SalesforceController(ILogger<SalesforceController> logger, SalesforceService salesforceService)
        {
            _logger = logger;
            _salesforceService = salesforceService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> Users(
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] string? search,
            [FromQuery] string? roleId,
            [FromQuery] string? status,
            CancellationToken cancellationToken)
        {
            try
            {
                var users = await _salesforceService.GetUsersAsync(
                    sortBy,
                    sortDirection,
                    search,
                    roleId,
                    status,
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
                var roles = await _salesforceService.GetRolesAsync(cancellationToken);
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
                await _salesforceService.UpdateUserActiveStatusAsync(
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
