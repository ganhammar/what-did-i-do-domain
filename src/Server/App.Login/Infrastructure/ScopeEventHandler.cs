using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace App.Login.Infrastructure;

public class ScopeEventHandler : IOpenIddictServerHandler<OpenIddictServerEvents.GenerateTokenContext>
{
  public ValueTask HandleAsync(OpenIddictServerEvents.GenerateTokenContext context)
  {
    var scopes = context.Principal.GetClaims("oi_scp");

    context.Principal.AddClaims("scope", scopes);

    return ValueTask.CompletedTask;
  }
}
