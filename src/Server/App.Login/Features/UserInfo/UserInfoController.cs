using App.Login.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;

namespace App.Login.Features.UserInfo;

public class UserInfoController : ApiControllerBase
{
  private readonly IMediator _mediator;

  public UserInfoController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
  [HttpGet($"~/{Constants.BasePath}/connect/userinfo")]
  [HttpPost($"~/{Constants.BasePath}/connect/userinfo")]
  [Produces("application/json")]
  public async Task<IActionResult> UserInfo(UserInfoQuery.Query query)
  {
    var result = await _mediator.Send(query);

    if (result.IsValid == false)
    {
      return Challenge(new AuthenticationProperties(
        result.Errors.ToDictionary(x => x.ErrorCode, x => x.ErrorMessage)!),
        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    return Ok(result.Result!);
  }
}
