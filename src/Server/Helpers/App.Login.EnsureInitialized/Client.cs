namespace App.Login.EnsureInitialized;

public class Client
{
  public string? Id { get; set; }
  public string? Secret { get; set; }
  public List<Uri> RedirectUris { get; set; } = new();
  public List<Uri> PostLogoutRedirectUris { get; set; } = new();
  public List<string> Scopes { get; set; } = new();
}
