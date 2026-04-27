using System.Text.Json.Serialization;

namespace SalesforceManager.Services.Salesforce.Models
{
    internal sealed class SalesforceTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("instance_url")]
        public string InstanceUrl { get; set; } = string.Empty;
    }

    internal sealed class SalesforceUsersResponse
    {
        [JsonPropertyName("records")]
        public List<SalesforceUserRecord> Records { get; set; } = [];
    }

    internal sealed class SalesforceRolesResponse
    {
        [JsonPropertyName("records")]
        public List<SalesforceRoleRecord> Records { get; set; } = [];
    }
}
