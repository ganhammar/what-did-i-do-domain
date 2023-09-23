using OpenIddict.Abstractions;

namespace App.Login.EnsureInitialized;

public class ClientOptions
{
  public List<OpenIddictApplicationDescriptor>? Clients { get; set; }
}
