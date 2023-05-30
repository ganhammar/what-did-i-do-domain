using OpenIddict.Abstractions;

namespace App.Login.EnsureInitialized;

public class ScopeOptions
{
  public List<OpenIddictScopeDescriptor>? Scopes { get; set; }
}
