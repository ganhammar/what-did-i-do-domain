using Amazon.CDK;
using Microsoft.Extensions.Configuration;

namespace AppStack;

public class Program
{
  protected static readonly IConfiguration Configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

  static void Main(string[] args)
  {
    var app = new App(null);

    new AppStack(app, "what-did-i-do-stack", new StackProps
    {
      Env = new Amazon.CDK.Environment
      {
        Region = "eu-north-1",
      },
    }, Configuration);

    app.Synth();
  }
}
