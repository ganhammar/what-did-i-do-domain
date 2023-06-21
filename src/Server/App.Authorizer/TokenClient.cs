using System.Text.Json;
using AWS.Lambda.Powertools.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace App.Authorizer;

public class TokenClient
{
  private readonly HttpClient _httpClient;
  private readonly IMemoryCache _memoryCache;

  private const string _cacheKey = "INTERNAL_TOKEN";

  public TokenClient(HttpClient httpClient, IMemoryCache memoryCache)
  {
    _httpClient = httpClient;
    _memoryCache = memoryCache;
  }

  public async Task<IntrospectionResult> Validate(AuthorizationOptions authorizationOptions, string token)
  {
    ArgumentNullException.ThrowIfNull(authorizationOptions.Issuer);

    var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
      $"{authorizationOptions.Issuer}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());

    var config = await configurationManager.GetConfigurationAsync();

    token = token.Replace("Bearer ", "");

    var introspectionToken = await GetTokenForIntrospection(authorizationOptions, config.TokenEndpoint);

    var request = new HttpRequestMessage(HttpMethod.Get, $"{config.IntrospectionEndpoint}?token={token}");
    request.Headers.Add("Authorization", $"Bearer {introspectionToken}");

    var response = await _httpClient.SendAsync(request);

    if (response.IsSuccessStatusCode)
    {
      var contentStream = await response.Content.ReadAsStreamAsync();
      var introspectionResult = await JsonSerializer.DeserializeAsync<IntrospectionResult>(contentStream);

      ArgumentNullException.ThrowIfNull(authorizationOptions.Audiences);

      if (introspectionResult?.Active == true && introspectionResult?.TokenUsage == "access_token"
        && introspectionResult?.Audience != default && authorizationOptions.Audiences.Contains(introspectionResult?.Audience!))
      {
        Logger.LogInformation($"Token successfully validated, result: {JsonSerializer.Serialize(introspectionResult)}");
        return introspectionResult!;
      }

      Logger.LogInformation($"Token could not be validated, result: {JsonSerializer.Serialize(introspectionResult)}");
    }

    throw new Exception("Unauthorized");
  }

  private async Task<string> GetTokenForIntrospection(AuthorizationOptions authorizationOptions, string uri)
  {
    if (_memoryCache.TryGetValue(_cacheKey, out var token))
    {
      Logger.LogInformation("Internal token found in cache, reusing");
      return (string)token;
    }

    Logger.LogInformation("Internal token not found in cache, requesting");
    ArgumentNullException.ThrowIfNull(authorizationOptions.ClientId);
    ArgumentNullException.ThrowIfNull(authorizationOptions.ClientSecret);

    var form = new FormUrlEncodedContent(new[]
    {
      new KeyValuePair<string, string>("client_id", authorizationOptions.ClientId!),
      new KeyValuePair<string, string>("client_secret", authorizationOptions.ClientSecret!),
      new KeyValuePair<string, string>("grant_type", "client_credentials"),
    });

    var response = await _httpClient.PostAsync(uri, form);

    if (response.IsSuccessStatusCode)
    {
      var contentStream = await response.Content.ReadAsStreamAsync();
      var tokenResult = await JsonSerializer.DeserializeAsync<TokenResult>(contentStream);

      if (tokenResult?.AccessToken != default)
      {
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(Convert.ToDouble(tokenResult.ExpiresIn - 60));
        _memoryCache.Set(_cacheKey, tokenResult.AccessToken, expiresAt);

        Logger.LogInformation($"Internal token fetched, cached until {expiresAt}");

        return tokenResult.AccessToken;
      }
    }

    throw new Exception("Could not get token for introspection");
  }
}
