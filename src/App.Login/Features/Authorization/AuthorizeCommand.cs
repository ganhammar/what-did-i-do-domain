using System.Security.Claims;
using App.Login.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace App.Login.Features.Authorization;

public class AuthorizeCommand
{
  public class Command : IRequest<IResponse<ClaimsPrincipal>>
  {
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator(IHttpContextAccessor httpContextAccessor)
    {
      RuleFor(x => x)
        .Cascade(CascadeMode.Stop)
        .Must((command) =>
        {
          var request = httpContextAccessor.HttpContext?.GetOpenIddictServerRequest();

          return request != default;
        })
        .WithErrorCode("NoAuthorizationRequestInProgress")
        .WithMessage("No authorization request in progress")
        .Must((command) =>
        {
          var httpContext = httpContextAccessor.HttpContext;
          var request = httpContext!.GetOpenIddictServerRequest();

          return httpContext!.User?.Identity?.IsAuthenticated == true
            || request!.HasPrompt(Prompts.None) == false;
        })
        .WithErrorCode(Errors.LoginRequired)
        .WithMessage("Login required");
    }
  }

  public class CommandHandler : Handler<Command, IResponse<ClaimsPrincipal>>
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<DynamoDbUser> _userManager;
    private readonly SignInManager<DynamoDbUser> _signInManager;
    private readonly IOpenIddictScopeManager _scopeManager;

    public CommandHandler(
      IHttpContextAccessor httpContextAccessor,
      UserManager<DynamoDbUser> userManager,
      SignInManager<DynamoDbUser> signInManager,
      IOpenIddictScopeManager scopeManager)
    {
      _httpContextAccessor = httpContextAccessor;
      _userManager = userManager;
      _signInManager = signInManager;
      _scopeManager = scopeManager;
    }

    public override async Task<IResponse<ClaimsPrincipal>> Handle(
      Command request, CancellationToken cancellationToken)
    {
      var httpContext = _httpContextAccessor.HttpContext;
      var openIddictRequest = httpContext!.GetOpenIddictServerRequest();

      if (httpContext!.User?.Identity?.IsAuthenticated != true)
      {
        return Response<ClaimsPrincipal>(new());
      }

      var user = await _userManager.GetUserAsync(httpContext.User);
      var principal = await _signInManager.CreateUserPrincipalAsync(user);

      var scopes = openIddictRequest!.GetScopes();
      principal.SetScopes(scopes);
      principal.SetResources(await _scopeManager.ListResourcesAsync(scopes).ToListAsync());

      foreach (var claim in principal.Claims)
      {
        claim.SetDestinations(GetDestinations(claim, principal));
      }

      return Response(principal);
    }

    private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
      // Note: by default, claims are NOT automatically included in the access and identity tokens.
      // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
      // whether they should be included in access tokens, in identity tokens or in both.

      switch (claim.Type)
      {
        case Claims.Name:
          yield return Destinations.AccessToken;

          if (principal.HasScope(Scopes.Profile))
            yield return Destinations.IdentityToken;

          yield break;

        case Claims.Email:
          yield return Destinations.AccessToken;

          if (principal.HasScope(Scopes.Email))
            yield return Destinations.IdentityToken;

          yield break;

        case Claims.Role:
          yield return Destinations.AccessToken;

          if (principal.HasScope(Scopes.Roles))
            yield return Destinations.IdentityToken;

          yield break;

        // Never include the security stamp in the access and identity tokens, as it's a secret value.
        case "AspNet.Identity.SecurityStamp": yield break;

        default:
          yield return Destinations.AccessToken;
          yield break;
      }
    }
  }
}
