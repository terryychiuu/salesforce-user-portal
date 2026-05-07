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

        public Task<IReadOnlyList<SalesforceUserDto>> GetUsers(CancellationToken cancellationToken = default)
        {
            return GetUsers(null, null, null, null, null, null, cancellationToken);
        }

        public async Task<IReadOnlyList<SalesforceUserDto>> GetUsers(
            string? sortBy,
            string? sortDirection,
            string? search,
            IReadOnlyList<string>? roleId,
            string? status,
            string? lastLogin,
            CancellationToken cancellationToken = default)
        {
            var usersResponse = await _salesforceApiClient.GetUsers(
                sortBy,
                sortDirection,
                search,
                roleId,
                status,
                lastLogin,
                cancellationToken
            );

            if (usersResponse?.Records == null)
                return Array.Empty<SalesforceUserDto>();

            return usersResponse.Records
                .Select(SalesforceUserMapper.ToDto)
                .ToList();
        }

        public async Task<IReadOnlyList<SalesforceRoleDto>> GetRoles(CancellationToken cancellationToken = default)
        {
            var rolesResponse = await _salesforceApiClient.GetRoles(cancellationToken);

            if (rolesResponse?.Records == null)
                return Array.Empty<SalesforceRoleDto>();

            return rolesResponse.Records
                .Select(SalesforceRoleMapper.ToDto)
                .ToList();
        }

        public Task UpdateUserActiveStatus(string userId, bool isActive, CancellationToken cancellationToken = default)
        {
            return _salesforceApiClient.PatchUserIsActive(userId, isActive, cancellationToken);
        }

        public async Task<SalesforceUserDto?> GetUserById(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _salesforceApiClient.GetUserById(userId, cancellationToken);
            return user is null ? null : SalesforceUserMapper.ToDto(user);
        }
    }
}
