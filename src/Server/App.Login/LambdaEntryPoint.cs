using Amazon.Lambda.AspNetCoreServer;

namespace App.Login;

public class LambdaEntryPoint : APIGatewayProxyFunction
{
  protected override void Init(IWebHostBuilder builder)
  {
    builder
      .ConfigureAppConfiguration(builder =>
      {
        builder.AddSystemsManager("/WDID/Login");
      })
      .UseStartup<Startup>();
  }

  protected override void Init(IHostBuilder builder) { }
}
