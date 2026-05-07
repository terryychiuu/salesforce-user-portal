using System.Collections.Generic;
namespace SalesforceManager.Models
{
    public class HomeViewModel
    {
        public IReadOnlyList<SalesforceUserDto> Users { get; set; } = new List<SalesforceUserDto>();
        public IReadOnlyList<SalesforceRoleDto> Roles { get; set; } = new List<SalesforceRoleDto>();
        public IReadOnlyList<string> AdminUsernames { get; set; } = new List<string>();
    }
}