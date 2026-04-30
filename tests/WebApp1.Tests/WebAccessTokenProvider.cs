using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebApp1.Tests;

public class WebAccessTokenProvider
{
    private const string DefaultUsername = "test-user";

    private const string DefaultPassword = "password";

    private readonly ILogger<WebAccessTokenProvider> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    private class AccessTokenItem
    {
        public string AccessToken { get; set; } = string.Empty;

        public DateTime ExpiresIn { get; set; }
    }

    public WebAccessTokenProvider(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _logger = loggerFactory.CreateLogger<WebAccessTokenProvider>();
    }

    public async Task<string> GetApiToken(string clientId, string scope, string secret)
    {
        _logger.LogDebug("GetApiToken new from secure token server for {ClientId}", clientId);

        var username = _configuration["Keycloak:Username"] ?? DefaultUsername;
        var password = _configuration["Keycloak:Password"] ?? DefaultPassword;

        var newAccessToken = await GetInternalApiToken(clientId, scope, secret, username, password);

        return newAccessToken.AccessToken;
    }

    private async Task<AccessTokenItem> GetInternalApiToken(string clientId, string scope, string secret, string username, string password)
    {
        try
        {
            var discoveryDocumentResponse = await _httpClient.GetAsync(
                _configuration["Keycloak:Authority"] + "/.well-known/openid-configuration");

            var discoveryDocument = await discoveryDocumentResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var tokenEndpoint = discoveryDocument is null || !discoveryDocument.TryGetValue("token_endpoint", out var endpoint)
                ? throw new InvalidOperationException("Keycloak discovery document does not contain a token endpoint.")
                : endpoint;

            using var tokenRequestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = clientId,
                ["client_secret"] = secret,
                ["scope"] = scope,
                ["username"] = username,
                ["password"] = password,
            });

            var tokenResponse = await _httpClient.PostAsync(tokenEndpoint, tokenRequestContent);

            tokenResponse.EnsureSuccessStatusCode();

            var tokenPayload = await tokenResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

            if (tokenPayload is null || string.IsNullOrWhiteSpace(tokenPayload.AccessToken))
            {
                throw new InvalidOperationException("Keycloak returned an empty access token.");
            }

            return new AccessTokenItem
            {
                ExpiresIn = DateTime.UtcNow.AddSeconds(tokenPayload.ExpiresIn),
                AccessToken = tokenPayload.AccessToken,
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception while acquiring access token for client {ClientId}", clientId);
            throw new InvalidOperationException("Failed to acquire access token.", e);
        }
    }
}

internal sealed class AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}