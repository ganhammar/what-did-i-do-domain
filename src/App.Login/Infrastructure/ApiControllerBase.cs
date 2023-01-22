using Microsoft.AspNetCore.Mvc;

namespace App.Login.Infrastructure;

public abstract class ApiControllerBase : Controller
{
  public IActionResult Respond<TReturnType>(IResponse<TReturnType> response)
  {
    if (response.IsValid)
    {
      return Ok(response.Result);
    }

    return BadRequest(response.Errors);
  }

  public IActionResult Respond(IResponse response)
  {
    if (response.IsValid)
    {
      return NoContent();
    }

    return BadRequest(response.Errors);
  }
}
