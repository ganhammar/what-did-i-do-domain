using App.Login.Infrastructure;
using App.Login.Infrastructure.Validators;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace App.Login.Features.User;

public class GetTwoFactorProvidersQuery
{
  public class Query : IRequest<IResponse<List<string>>>
  {
  }

  public class QueryValidator : AbstractValidator<Query>
  {
    public QueryValidator(SignInManager<DynamoDbUser> signInManager)
    {
      RuleFor(x => x)
        .MustAsync(async (query, cancellationToken) =>
        {
          var user = await signInManager.GetTwoFactorAuthenticationUserAsync();

          return user != default;
        })
        .WithErrorCode(nameof(ErrorCodes.NoLoginAttemptInProgress))
        .WithMessage(ErrorCodes.NoLoginAttemptInProgress);
    }
  }

  public class QueryHandler : Handler<Query, IResponse<List<string>>>
  {
    private readonly UserManager<DynamoDbUser> _userManager;
    private readonly SignInManager<DynamoDbUser> _signInManager;

    public QueryHandler(
      UserManager<DynamoDbUser> userManager,
      SignInManager<DynamoDbUser> signInManager)
    {
      _userManager = userManager;
      _signInManager = signInManager;
    }

    public override async Task<IResponse<List<string>>> Handle(
      Query request, CancellationToken cancellationToken)
    {
      var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
      var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);

      return Response(providers.ToList());
    }
  }
}
