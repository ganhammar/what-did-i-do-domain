using Microsoft.AspNetCore.Mvc;

namespace App.Login.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{
  private readonly ILogger<HelloController> _logger;

  public HelloController(ILogger<HelloController> logger)
  {
    _logger = logger;
  }

  [HttpGet(Name = "Hello")]
  public string Get()
  {
    return "world";
  }
}
