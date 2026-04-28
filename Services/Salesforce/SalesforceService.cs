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

        public Task<IReadOnlyList<SalesforceUserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            return GetUsersAsync(null, null, cancellationToken);
        }

        public async Task<IReadOnlyList<SalesforceUserDto>> GetUsersAsync(
            string? sortBy,
            string? sortDirection,
            CancellationToken cancellationToken = default)
        {
            var usersResponse = await _salesforceApiClient.GetUsersAsync(sortBy, sortDirection, cancellationToken);

            if (usersResponse?.Records == null)
                return Array.Empty<SalesforceUserDto>();

            return usersResponse.Records
                .Select(SalesforceUserMapper.ToDto)
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
