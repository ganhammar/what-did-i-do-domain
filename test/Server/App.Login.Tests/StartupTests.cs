using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace App.Login.Tests;

public class StartupTests
{
  [Fact]
  public async Task Should_BeNotFound_When_TryingToGetRootPath()
  {
    var host = new HostBuilder()
      .ConfigureWebHost(webBuilder =>
      {
        webBuilder.UseEnvironment("Development");
        webBuilder.UseStartup<Startup>();
        webBuilder.UseTestServer();
      })
      .Build();

    await host.StartAsync();

    var client = host.GetTestServer().CreateClient();

    var response = await client.GetAsync("/");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }
}
