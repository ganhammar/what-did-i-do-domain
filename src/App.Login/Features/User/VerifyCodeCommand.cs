using App.Login.Infrastructure;
using App.Login.Infrastructure.Validators;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace App.Login.Features.User;

public class VerifyCodeCommand
{
  public class Command : IRequest<IResponse<SignInResult>>
  {
    public string? Provider { get; set; }
    public string? Code { get; set; }
    public bool RememberBrowser { get; set; }
    public bool RememberMe { get; set; }
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator(
      SignInManager<DynamoDbUser> signInManager,
      UserManager<DynamoDbUser> userManager)
    {
      RuleFor(x => x)
        .MustAsync(async (query, cancellationToken) =>
        {
          var user = await signInManager.GetTwoFactorAuthenticationUserAsync();

          return user != default;
        })
        .WithErrorCode(nameof(ErrorCodes.NoLoginAttemptInProgress))
        .WithMessage(ErrorCodes.NoLoginAttemptInProgress);

      RuleFor(x => x.Provider)
        .NotEmpty()
        .MustAsync(async (provider, cancellationToken) =>
        {
          var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
          if (user == default)
          {
            return false;
          }

          var providers = await userManager.GetValidTwoFactorProvidersAsync(user);

          return providers.Contains(provider);
        })
        .WithErrorCode(nameof(ErrorCodes.TwoFactorProviderNotValid))
        .WithMessage(ErrorCodes.TwoFactorProviderNotValid);

      RuleFor(x => x.Code)
        .NotEmpty();
    }
  }

  public class CommandHandler : Handler<Command, IResponse<SignInResult>>
  {
    private readonly UserManager<DynamoDbUser> _userManager;
    private readonly SignInManager<DynamoDbUser> _signInManager;

    public CommandHandler(
      UserManager<DynamoDbUser> userManager,
      SignInManager<DynamoDbUser> signInManager)
    {
      _userManager = userManager;
      _signInManager = signInManager;
    }

    public override async Task<IResponse<SignInResult>> Handle(
      Command request, CancellationToken cancellationToken)
    {
      var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
      var result = await _signInManager
        .TwoFactorSignInAsync(request.Provider, request.Code, request.RememberMe, request.RememberBrowser);

      return Response(result);
    }
  }
}
