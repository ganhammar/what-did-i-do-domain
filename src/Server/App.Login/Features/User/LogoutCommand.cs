using App.Login.Infrastructure;
using App.Login.Infrastructure.Validators;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace App.Login.Features.User;

public class LogoutCommand
{
  public class Command : IRequest<IResponse>
  {
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator(
      UserManager<DynamoDbUser> userManager,
      IHttpContextAccessor httpContextAccessor)
    {
      RuleFor(x => x)
        .MustAsync(async (command, cancellationToken) =>
        {
          if (httpContextAccessor.HttpContext?.User == default)
          {
            return false;
          }

          var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext?.User);

          return user != default;
        })
        .WithErrorCode(nameof(ErrorCodes.NoLoggedInUser))
        .WithMessage(ErrorCodes.NoLoggedInUser);
    }
  }

  public class CommandHandler : Handler<Command, IResponse>
  {
    private readonly SignInManager<DynamoDbUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CommandHandler(
      SignInManager<DynamoDbUser> signInManager,
      IHttpContextAccessor httpContextAccessor)
    {
      _signInManager = signInManager;
      _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IResponse> Handle(
      Command request, CancellationToken cancellationToken)
    {
      await _signInManager.SignOutAsync();

      return Response();
    }
  }
}
