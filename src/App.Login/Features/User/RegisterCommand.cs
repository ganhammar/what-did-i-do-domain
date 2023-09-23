﻿using System.Web;
using App.Login.Features.Email;
using App.Login.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace App.Login.Features.User;

public class RegisterCommand
{
  public class Command : IRequest<IResponse<UserDto>>
  {
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? ReturnUrl { get; set; }
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator()
    {
      RuleFor(x => x.Password)
        .NotEmpty();

      RuleFor(x => x.ReturnUrl)
        .NotEmpty();

      When(x => string.IsNullOrEmpty(x.UserName), () =>
      {
        RuleFor(x => x.Email)
          .NotEmpty()
          .EmailAddress();
      });

      When(x => string.IsNullOrEmpty(x.Email), () =>
      {
        RuleFor(x => x.UserName)
          .NotEmpty();
      });
    }
  }

  public class CommandHandler : Handler<Command, IResponse<UserDto>>
  {
    private readonly UserManager<DynamoDbUser> _userManager;
    private readonly IEmailSender _emailSender;

    public CommandHandler(
      UserManager<DynamoDbUser> userManager,
      IEmailSender emailSender)
    {
      _userManager = userManager;
      _emailSender = emailSender;
    }

    public override async Task<IResponse<UserDto>> Handle(
      Command request, CancellationToken cancellationToken)
    {
      var user = new DynamoDbUser
      {
        UserName = request.UserName ?? request.Email,
        Email = request.Email,
        EmailConfirmed = false,
        PhoneNumberConfirmed = false,
      };

      var result = await _userManager.CreateAsync(user, request.Password);

      if (result.Succeeded)
      {
        await SendConfirmationEmail(user, request.ReturnUrl);
      }
      else
      {
        return Response<UserDto>(new(), result.Errors.Select(x => new ValidationFailure
        {
          ErrorCode = x.Code,
          ErrorMessage = x.Description,
        }));
      }

      return Response(UserMapper.ToDto(user));
    }

    private async Task SendConfirmationEmail(DynamoDbUser user, string? returnUrl)
    {
      var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      var url = $"{Constants.Origin}{Constants.BasePath}/user/confirm"
        + $"?UserId={user.Id}&Token={HttpUtility.UrlEncode(token)}"
        + $"&ReturnUrl={HttpUtility.UrlEncode(returnUrl)}";

      var body = $"Follow the link below to confirm your WDID account:<br /><a href=\"{url}\">{url}</a>";

      await _emailSender.Send(user.Email, "Confirm WDID Account", body);
    }
  }
}
