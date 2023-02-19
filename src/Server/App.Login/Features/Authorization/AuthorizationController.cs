using App.Login.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;

namespace App.Login.Features.Authorization;

public class AuthorizationController : ApiControllerBase
{
  private readonly IMediator _mediator;

  public AuthorizationController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet("~/connect/authorize")]
  public async Task<IActionResult> Authorize(AuthorizeCommand.Command command)
  {
    var result = await _mediator.Send(command);

    if (result.IsValid == false)
    {
      return Forbid(new AuthenticationProperties(
          result.Errors.ToDictionary(x => x.ErrorCode, x => x.ErrorMessage)!),
          OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    if (result.Result?.Identity?.IsAuthenticated != true)
    {
      return Challenge();
    }

    return SignIn(
        result.Result!,
        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
  }

  [HttpGet("~/connect/LogoutCommand")]
  public async Task<IActionResult> LogoutCommand(LogoutCommand.Command command)
  {
    await _mediator.Send(command);

    return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
  }

  [HttpPost("~/connect/token")]
  [Produces("application/json")]
  public async Task<IActionResult> ExchangeCommand(ExchangeCommand.Command command)
  {
    var result = await _mediator.Send(command);

    if (result.IsValid == false)
    {
      return Forbid(new AuthenticationProperties(
          result.Errors.ToDictionary(x => x.ErrorCode, x => x.ErrorMessage)!),
          OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    return SignIn(result.Result!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
  }
}
