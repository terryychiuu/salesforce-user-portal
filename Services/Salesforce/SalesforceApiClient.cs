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

        internal async Task<SalesforceUsersResponse> GetUsers(
            string? sortBy = null,
            string? sortDirection = null,
            string? search = null,
            IReadOnlyList<string>? roleId = null,
            string? status = null,
            string? lastLogin = null,
            CancellationToken cancellationToken = default)
        {
            var tokenResponse = await Authenticate(cancellationToken);

            var orderByField = ResolveUsersOrderByField(sortBy);
            var orderByDirection = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? "DESC"
                : "ASC";
                
            var conditions = new List<string>();
            var normalizedSearch = search?.Trim();
            string? lastLoginCondition = lastLogin?.Trim().ToLowerInvariant() switch
            {
                "within-1d" => "LastLoginDate >= LAST_N_DAYS:1",
                "within-7d" => "LastLoginDate >= LAST_N_DAYS:7",
                "within-30d" => "LastLoginDate >= LAST_N_DAYS:30",
                "older-30d" => "LastLoginDate < LAST_N_DAYS:30",
                "older-90d" => "LastLoginDate < LAST_N_DAYS:90",
                null => null,
                "" => null,
                _ => null
            };
            string? isActive = status?.Trim().ToLowerInvariant() switch
            {
                "active" => "true",
                "inactive" => "false",
                "all" => null,
                null => null,
                "" => null,
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                var escapedSearch = EscapeSoqlStringLiteral(normalizedSearch);
                conditions.Add($"(Name LIKE '%{escapedSearch}%' OR Username LIKE '%{escapedSearch}%')");
            }

            var roleIds = (roleId ?? Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
            if (roleIds.Count == 1)
            {
                conditions.Add($"UserRoleId = '{EscapeSoqlStringLiteral(roleIds[0])}'");
            }
            else if (roleIds.Count > 1)
            {
                var inList = string.Join(", ", roleIds.Select(id => $"'{EscapeSoqlStringLiteral(id)}'"));
                conditions.Add($"UserRoleId IN ({inList})");
            }

            if (!string.IsNullOrWhiteSpace(isActive))
            {
                conditions.Add($"IsActive = {isActive}");
            }

            if (!string.IsNullOrWhiteSpace(lastLogin))
            {
                conditions.Add($"{lastLoginCondition}");
            }
            
            var whereClause = conditions.Count > 0
                ? $"WHERE {string.Join(" AND ", conditions)} "
                : string.Empty;
            var soql = "" +
                "SELECT Id, Name, Alias, Username, LastLoginDate, UserRoleId, UserRole.Name, IsActive " +
                "FROM User " +
                whereClause +
                $"ORDER BY {orderByField} {orderByDirection}";
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

        private static string ResolveUsersOrderByField(string? sortBy)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "username" => "Username",
                "lastlogindate" => "LastLoginDate",
                "role" => "UserRole.Name",
                "active" => "IsActive",
                _ => "Name"
            };
        }

        private static string EscapeSoqlStringLiteral(string value)
        {
            return value.Replace("\\", "\\\\").Replace("'", "\\'");
        }

        internal async Task PatchUserIsActive(string userId, bool isActive, CancellationToken cancellationToken = default)
        {
            var tokenResponse = await Authenticate(cancellationToken);
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

        internal async Task<SalesforceRolesResponse> GetRoles(CancellationToken cancellationToken = default)
        {
            var tokenResponse = await Authenticate(cancellationToken);
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
        
        private async Task<SalesforceTokenResponse> Authenticate(CancellationToken cancellationToken)
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
