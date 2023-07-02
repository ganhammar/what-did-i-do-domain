using System.Text.Json.Serialization;

namespace App.Authorizer;

public class IntrospectionResult
{
  [JsonPropertyName("active")]
  public bool Active { get; set; }
  [JsonPropertyName("aud")]
  public string? Audience { get; set; }
  [JsonPropertyName("token_usage")]
  public string? TokenUsage { get; set; }
  [JsonPropertyName("sub")]
  public string? Subject { get; set; }
  [JsonPropertyName("scope")]
  public string? Scope { get; set; }
  [JsonPropertyName("email")]
  public string? Email { get; set; }
}
