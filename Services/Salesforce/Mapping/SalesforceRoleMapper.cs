using SalesforceManager.Models;
using SalesforceManager.Services.Salesforce.Models;

namespace SalesforceManager.Services.Salesforce.Mapping
{
    internal static class SalesforceRoleMapper
    {
        public static SalesforceRoleDto ToDto(SalesforceRoleRecord role) => new()
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty
        };
    }
}
