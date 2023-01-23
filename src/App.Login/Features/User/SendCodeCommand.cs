using App.Login.Features.Email;
using App.Login.Infrastructure;
using App.Login.Infrastructure.Validators;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace App.Login.Features.User;

public class SendCodeCommand
{
  public class Command : IRequest<IResponse>
  {
    public string? Provider { get; set; }
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
    }
  }

  public class CommandHandler : Handler<Command, IResponse>
  {
    private readonly UserManager<DynamoDbUser> _userManager;
    private readonly SignInManager<DynamoDbUser> _signInManager;
    private readonly IEmailSender _emailSender;

    public CommandHandler(
      UserManager<DynamoDbUser> userManager,
      SignInManager<DynamoDbUser> signInManager,
      IEmailSender emailSender)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _emailSender = emailSender;
    }

    public override async Task<IResponse> Handle(
      Command request, CancellationToken cancellationToken)
    {
      var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
      var code = await _userManager.GenerateTwoFactorTokenAsync(user, request.Provider);

      var message = $"Your security code is: {code}";

      switch (request.Provider)
      {
        case "Email":
          await _emailSender.Send(await _userManager.GetEmailAsync(user), "Security Code - WDID", message);
          break;
      }

      return Response();
    }
  }
}
