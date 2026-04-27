using Microsoft.Extensions.Options;
using SalesforceManager.Services.Salesforce.Configuration;
using SalesforceManager.Services.Salesforce.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SalesforceManager.Services.Salesforce
{
    public class SalesforceApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly SalesforceConfig _config;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SalesforceApiClient(HttpClient httpClient, IOptions<SalesforceConfig> options)
        {
            _httpClient = httpClient;
            _config = options.Value;
        }

        internal async Task<SalesforceUsersResponse> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            var tokenResponse = await AuthenticateAsync(cancellationToken);
            var soql = "" +
                "SELECT Id, Name, Alias, Username, LastLoginDate, UserRoleId, IsActive " +
                "FROM User " +
                "ORDER BY Name";
            var queryUrl = $"{_config.Url}/services/data/v53.0/query?q={Uri.EscapeDataString(soql)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(payload);

            return JsonSerializer.Deserialize<SalesforceUsersResponse>(payload, JsonOptions) 
                ?? new SalesforceUsersResponse();
        }

        internal async Task<SalesforceRolesResponse> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            var tokenResponse = await AuthenticateAsync(cancellationToken);
            var soql = "" +
                "SELECT Id, Name " +
                "FROM UserRole " +
                "ORDER BY Name";
            var queryUrl = $"{_config.Url}/services/data/v53.0/query?q={Uri.EscapeDataString(soql)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            // Console.WriteLine(payload);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(payload);

            return JsonSerializer.Deserialize<SalesforceRolesResponse>(payload, JsonOptions)
                ?? new SalesforceRolesResponse();
        }

        internal async Task PatchUserIsActiveAsync(string userId, bool isActive, CancellationToken cancellationToken = default)
        {
            var tokenResponse = await AuthenticateAsync(cancellationToken);
            var patchUrl = $"{tokenResponse.InstanceUrl}/services/data/v53.0/sobjects/User/{Uri.EscapeDataString(userId)}";
            using var request = new HttpRequestMessage(HttpMethod.Patch, patchUrl)
            {
                Content = JsonContent.Create(new
                {
                    IsActive = isActive
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(payload);
        }
        
        private async Task<SalesforceTokenResponse> AuthenticateAsync(CancellationToken cancellationToken)
        {
            var tokenEndpoint = $"{_config.Url}/services/oauth2/token";
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = _config.Username,
                ["password"] = _config.Password,
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret
            };

            using var content = new FormUrlEncodedContent(form);
            using var response = await _httpClient.PostAsync(tokenEndpoint, content, cancellationToken);

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(payload);

            var tokenResponse = JsonSerializer.Deserialize<SalesforceTokenResponse>(payload, JsonOptions)
                ?? throw new InvalidOperationException("Salesforce authentication did not return a valid access token.");
            tokenResponse.InstanceUrl = string.IsNullOrWhiteSpace(tokenResponse.InstanceUrl)
                ? _config.Url
                : tokenResponse.InstanceUrl;

            return tokenResponse;
        }
    }
}
