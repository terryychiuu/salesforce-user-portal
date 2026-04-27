using SalesforceManager.Models;
using SalesforceManager.Services.Salesforce.Models;
using SalesforceManager.Services.Salesforce.Utilities;

namespace SalesforceManager.Services.Salesforce.Mapping
{
    internal static class SalesforceUserMapper
    {
        public static SalesforceUserDto ToDto(SalesforceUserRecord user) => new()
        {
            Id = user.Id,
            Name = user.Name ?? string.Empty,
            Username = user.Username ?? string.Empty,
            LastLoginDate = SalesforceDateTimeFormatter.FormatForDisplay(user.LastLoginDate),
            UserRoleId = user.UserRoleId ?? string.Empty,
            IsActive = user.IsActive
        };
    }
}
