using App.Login.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace App.Login.Features.User;

public class ResetPasswordCommand
{
  public class Command : IRequest<IResponse>
  {
    public string? UserId { get; set; }
    public string? Token { get; set; }
    public string? ReturnUrl { get; set; }
    public string? Password { get; set; }
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator()
    {
      RuleFor(x => x.UserId)
        .NotEmpty();

      RuleFor(x => x.Token)
        .NotEmpty();

      RuleFor(x => x.ReturnUrl)
        .NotEmpty();

      RuleFor(x => x.Password)
        .NotEmpty();
    }
  }

  public class CommandHandler : Handler<Command, IResponse>
  {
    private readonly UserManager<DynamoDbUser> _userManager;

    public CommandHandler(UserManager<DynamoDbUser> userManager)
    {
      _userManager = userManager;
    }

    public override async Task<IResponse> Handle(
      Command request, CancellationToken cancellationToken)
    {
      var user = await _userManager.FindByIdAsync(request.UserId);

      if (user != null)
      {
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);

        if (!result.Succeeded)
        {
          return Response(result.Errors.Select(x => new ValidationFailure
          {
            ErrorCode = x.Code,
            ErrorMessage = x.Description,
          }));
        }
      }

      return Response();
    }
  }
}
