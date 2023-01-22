using App.Login.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace App.Login.Features.Authorization;

public class LogoutCommand
{
  public class Command : IRequest<IResponse>
  {
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator()
    {
    }
  }

  public class CommandHandler : Handler<Command, IResponse>
  {
    private readonly SignInManager<DynamoDbUser> _signInManager;

    public CommandHandler(SignInManager<DynamoDbUser> signInManager)
    {
      _signInManager = signInManager;
    }

    public override async Task<IResponse> Handle(
        Command request, CancellationToken cancellationToken)
    {
      await _signInManager.SignOutAsync();

      return Response();
    }
  }
}
