using System.Text.Json.Serialization;

namespace SalesforceManager.Services.Salesforce.Models
{
    internal sealed class SalesforceUserRecord
    {
        [JsonPropertyName("Id")]
        public required string Id { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("Username")]
        public string? Username { get; set; }

        [JsonPropertyName("LastLoginDate")]
        public string? LastLoginDate { get; set; }

        [JsonPropertyName("UserRoleId")]
        public string? UserRoleId { get; set; }

        [JsonPropertyName("UserRole")]
        public SalesforceRoleRecord? UserRole { get; set; }

        [JsonPropertyName("IsActive")]
        public bool IsActive { get; set; }
    }

    internal sealed class SalesforceRoleRecord
    {
        [JsonPropertyName("Id")]
        public string? Id { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }
    }
}
