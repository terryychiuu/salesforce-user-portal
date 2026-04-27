namespace SalesforceManager.Models
{
    public class SalesforceUserDto
    {
        public required string Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string Username { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string LastLoginDate { get; internal set; } = string.Empty;
        
        public string UserRoleId { get; internal set; } = string.Empty;
        
        public bool IsActive { get; set; }
    }
}
