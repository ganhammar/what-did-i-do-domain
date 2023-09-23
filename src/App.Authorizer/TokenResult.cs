using System.Text.Json.Serialization;

namespace App.Authorizer;

public class TokenResult
{
  [JsonPropertyName("access_token")]
  public string? AccessToken { get; set; }
  [JsonPropertyName("expires_in")]
  public int? ExpiresIn { get; set; }
}
