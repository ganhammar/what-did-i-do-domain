using App.Login.Infrastructure;
using App.Login.Infrastructure.Validators;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace App.Login.Features.User;

public class LoginCommand
{
  public class Command : IRequest<IResponse<SignInResult>>
  {
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public bool RememberMe { get; set; }
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator(UserManager<DynamoDbUser> userManager)
    {
      When(x => string.IsNullOrEmpty(x.UserName), () =>
      {
        RuleFor(x => x.Email)
          .NotEmpty();

        When(x => string.IsNullOrEmpty(x.Email) == false, () =>
        {
          RuleFor(x => x.Email)
            .EmailAddress()
            .MustAsync(async (email, cancellationToken) =>
            {
              var user = await userManager.FindByEmailAsync(email);

              return user != default && user.EmailConfirmed;
            })
            .WithErrorCode(nameof(ErrorCodes.EmailUnconfirmed))
            .WithMessage(ErrorCodes.EmailUnconfirmed);
        });
      });

      When(x => string.IsNullOrEmpty(x.Email), () =>
      {
        RuleFor(x => x.UserName)
          .NotEmpty();
      });

      When(x => string.IsNullOrEmpty(x.Email) == false, () =>
      {
        RuleFor(x => x.UserName)
          .Empty();
      });

      RuleFor(x => x.Password)
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
      var userName = request.UserName;

      if (request.Email != default)
      {
        var user = await _userManager.FindByEmailAsync(request.Email);
        userName = user.UserName;
      }

      return Response(await _signInManager
        .PasswordSignInAsync(userName, request.Password, request.RememberMe, false));
    }
  }
}
