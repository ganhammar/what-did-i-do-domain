using System.Collections.Immutable;
using System.Security.Claims;
using App.Login.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
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

            return request!.IsClientCredentialsGrantType();
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

    public CommandHandler(
      IHttpContextAccessor httpContextAccessor,
      IOpenIddictApplicationManager applicationManager,
      IOpenIddictScopeManager scopeManager)
    {
      _httpContextAccessor = httpContextAccessor;
      _applicationManager = applicationManager;
      _scopeManager = scopeManager;
    }

    public override async Task<IResponse<ClaimsPrincipal>> Handle(
      Command request, CancellationToken cancellationToken)
    {
      var openIddictRequest = _httpContextAccessor.HttpContext!.GetOpenIddictServerRequest();
      var application = await _applicationManager.FindByClientIdAsync(openIddictRequest!.ClientId!);

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

    private IEnumerable<string> GetDestinations(Claim claim)
    {
      // Note: by default, claims are NOT automatically included in the access and identity tokens.
      // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
      // whether they should be included in access tokens, in identity tokens or in both.

      return claim.Type switch
      {
        Claims.Name or
        Claims.Subject
            => ImmutableArray.Create(Destinations.AccessToken, Destinations.IdentityToken),

        _ => ImmutableArray.Create(Destinations.AccessToken),
      };
    }
  }
}
