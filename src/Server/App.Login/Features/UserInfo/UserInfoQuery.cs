using App.Login.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace App.Login.Features.UserInfo;

public class UserInfoQuery
{
  public class Query : IRequest<IResponse<Dictionary<string, object>>>
  {
  }

  public class QueryValidator : AbstractValidator<Query>
  {
    public QueryValidator(
      UserManager<DynamoDbUser> userManager,
      IHttpContextAccessor httpContextAccessor)
    {
      RuleFor(x => x)
        .Cascade(CascadeMode.Stop)
        .Must((query) =>
          httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
        .WithErrorCode(Errors.InvalidToken)
        .WithMessage("The specified access token is bound to an account that no longer exists")
        .MustAsync(async (query, cancellationToken) =>
        {
          var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);
          return user != default;
        })
        .WithErrorCode(Errors.InvalidToken)
        .WithMessage("The specified access token is bound to an account that no longer exists");
    }
  }

  public class QueryHandler : Handler<Query, IResponse<Dictionary<string, object>>>
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<DynamoDbUser> _userManager;

    public QueryHandler(
      IHttpContextAccessor httpContextAccessor,
      UserManager<DynamoDbUser> userManager)
    {
      _httpContextAccessor = httpContextAccessor;
      _userManager = userManager;
    }

    public override async Task<IResponse<Dictionary<string, object>>> Handle(
      Query request, CancellationToken cancellationToken)
    {
      var principal = _httpContextAccessor.HttpContext!.User;
      var user = await _userManager.GetUserAsync(principal);

      var claims = new Dictionary<string, object>(StringComparer.Ordinal)
      {
        [Claims.Subject] = await _userManager.GetUserIdAsync(user)
      };

      if (principal.HasScope(Scopes.Email))
      {
        claims[Claims.Email] = await _userManager.GetEmailAsync(user);
        claims[Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
      }

      if (principal.HasScope(Scopes.Phone))
      {
        claims[Claims.PhoneNumber] = await _userManager.GetPhoneNumberAsync(user);
        claims[Claims.PhoneNumberVerified] = await _userManager.IsPhoneNumberConfirmedAsync(user);
      }

      if (principal.HasScope(Scopes.Roles))
      {
        claims[Claims.Role] = await _userManager.GetRolesAsync(user);
      }

      return Response(claims);
    }
  }
}
