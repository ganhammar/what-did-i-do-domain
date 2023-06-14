using System.Collections.Immutable;
using System.Security.Claims;
using App.Login.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace App.Login.Features.Authorization;

public class ExchangeCommand
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
          var request = httpContextAccessor.HttpContext!.GetOpenIddictServerRequest();

          return request!.IsClientCredentialsGrantType() || request!.IsAuthorizationCodeGrantType();
        })
        .WithErrorCode(Errors.UnsupportedGrantType)
        .WithMessage("The specified grant type is not supported");
    }
  }

  public class CommandHandler : Handler<Command, IResponse<ClaimsPrincipal>>
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly UserManager<DynamoDbUser> _userManager;
    private readonly SignInManager<DynamoDbUser> _signInManager;

    public CommandHandler(
      IHttpContextAccessor httpContextAccessor,
      IOpenIddictApplicationManager applicationManager,
      IOpenIddictScopeManager scopeManager,
      UserManager<DynamoDbUser> userManager,
      SignInManager<DynamoDbUser> signInManager)
    {
      _httpContextAccessor = httpContextAccessor;
      _applicationManager = applicationManager;
      _scopeManager = scopeManager;
      _userManager = userManager;
      _signInManager = signInManager;
    }

    public override async Task<IResponse<ClaimsPrincipal>> Handle(
      Command request, CancellationToken cancellationToken)
    {
      var openIddictRequest = _httpContextAccessor.HttpContext!.GetOpenIddictServerRequest()!;

      if (openIddictRequest.IsClientCredentialsGrantType())
      {
        return await HandleClientCredentials(openIddictRequest);
      }

      return await HandleAuthorizationCode(openIddictRequest);
    }

    private async Task<IResponse<ClaimsPrincipal>> HandleClientCredentials(OpenIddictRequest openIddictRequest)
    {
      var application = await _applicationManager.FindByClientIdAsync(openIddictRequest.ClientId!);

      if (application == default)
      {
        return Response<ClaimsPrincipal>(new(), new List<ValidationFailure>
        {
          new ValidationFailure("InvalidApplication", "The application is not valid in this context"),
        });
      }

      var identity = new ClaimsIdentity(
        TokenValidationParameters.DefaultAuthenticationType,
        Claims.Name, Claims.Role);

      // Use the client_id as the subject identifier.
      identity.SetClaim(Claims.Subject, (await _applicationManager.GetClientIdAsync(application))!);

      var name = await _applicationManager.GetDisplayNameAsync(application);

      if (name != default)
      {
        identity.SetClaim(Claims.Name, name);
      }

      identity.SetDestinations(static claim => claim.Type switch
      {
        Claims.Name when claim.Subject?.HasScope(Scopes.Profile) == true
          => new[] { Destinations.AccessToken, Destinations.IdentityToken },
        _ => new[] { Destinations.AccessToken },
      });

      var principal = new ClaimsPrincipal(identity);
      principal.SetScopes(openIddictRequest.GetScopes());
      principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

      foreach (var claim in principal.Claims)
      {
        claim.SetDestinations(GetDestinations(claim));
      }

      return Response(principal);
    }

    private async Task<IResponse<ClaimsPrincipal>> HandleAuthorizationCode(OpenIddictRequest openIddictRequest)
    {
      var result = await _httpContextAccessor.HttpContext!.AuthenticateAsync(
        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

      // Retrieve the user profile corresponding to the authorization code/refresh token.
      var user = await _userManager.FindByIdAsync(result.Principal?.GetClaim(Claims.Subject) ?? String.Empty);
      if (user is null)
      {
        return Response<ClaimsPrincipal>(new(), new List<ValidationFailure>
        {
          new ValidationFailure(nameof(ExchangeCommand.Command), "The token is no longer valid")
          {
            ErrorCode = Errors.InvalidGrant,
          },
        });
      }

      // Ensure the user is still allowed to sign in.
      if (!await _signInManager.CanSignInAsync(user))
      {
        return Response<ClaimsPrincipal>(new(), new List<ValidationFailure>
        {
          new ValidationFailure(nameof(ExchangeCommand.Command), "The user is no longer allowed to sign in")
          {
            ErrorCode = Errors.InvalidGrant,
          },
        });
      }

      var identity = new ClaimsIdentity(result.Principal!.Claims,
        authenticationType: TokenValidationParameters.DefaultAuthenticationType,
        nameType: Claims.Name,
        roleType: Claims.Role);

      identity.SetScopes(openIddictRequest.GetScopes());

      // Override the user claims present in the principal in case they
      // changed since the authorization code/refresh token was issued.
      identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
        .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
        .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
        .SetClaims(Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

      identity.SetDestinations(GetDestinations);

      // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
      return Response(new ClaimsPrincipal(identity));
    }

    private IEnumerable<string> GetDestinations(Claim claim)
    {
      // Note: by default, claims are NOT automatically included in the access and identity tokens.
      // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
      // whether they should be included in access tokens, in identity tokens or in both.

      switch (claim.Type)
      {
        case Claims.Name:
        case Claims.Subject:
          yield return Destinations.AccessToken;

          if (claim.Subject?.HasScope(Scopes.Profile) == true)
            yield return Destinations.IdentityToken;

          yield break;

        case Claims.Email:
          yield return Destinations.AccessToken;

          if (claim.Subject?.HasScope(Scopes.Email) == true)
            yield return Destinations.IdentityToken;

          yield break;

        case Claims.Role:
          yield return Destinations.AccessToken;

          if (claim.Subject?.HasScope(Scopes.Roles) == true)
            yield return Destinations.IdentityToken;

          yield break;

        case Claims.Scope:
          yield return Destinations.AccessToken;
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
