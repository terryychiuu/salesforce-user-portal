using SalesforceManager.Models;
using SalesforceManager.Services.Salesforce.Mapping;

namespace SalesforceManager.Services.Salesforce
{
    public class SalesforceService
    {
        private readonly SalesforceApiClient _salesforceApiClient;

        public SalesforceService(SalesforceApiClient salesforceApiClient)
        {
            _salesforceApiClient = salesforceApiClient;
        }

        public async Task<IReadOnlyList<SalesforceUserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            var usersResponse = await _salesforceApiClient.GetUsersAsync(cancellationToken);
            var rolesResponse = await _salesforceApiClient.GetRolesAsync(cancellationToken);

            if (usersResponse?.Records == null)
                return Array.Empty<SalesforceUserDto>();

            var roleNameById = (rolesResponse?.Records ?? [])
                .Where(role => !string.IsNullOrWhiteSpace(role.Id))
                .ToDictionary(
                    role => role.Id,
                    role => role.Name ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            return usersResponse.Records
                .Select(SalesforceUserMapper.ToDto)
                .Select(user =>
                {
                    if (!string.IsNullOrWhiteSpace(user.UserRoleId) &&
                        roleNameById.TryGetValue(user.UserRoleId, out var roleName) &&
                        !string.IsNullOrWhiteSpace(roleName))
                    {
                        user.UserRoleId = roleName;
                    }

                    return user;
                })
                .ToList();
        }

        public async Task<IReadOnlyList<SalesforceRoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            var rolesResponse = await _salesforceApiClient.GetRolesAsync(cancellationToken);

            if (rolesResponse?.Records == null)
                return Array.Empty<SalesforceRoleDto>();

            return rolesResponse.Records
                .Select(SalesforceRoleMapper.ToDto)
                .ToList();
        }

        public Task UpdateUserActiveStatusAsync(string userId, bool isActive, CancellationToken cancellationToken = default)
        {
            return _salesforceApiClient.PatchUserIsActiveAsync(userId, isActive, cancellationToken);
        }
    }
}
