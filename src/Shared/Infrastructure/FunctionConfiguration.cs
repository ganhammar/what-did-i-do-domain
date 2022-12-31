using Microsoft.Extensions.Configuration;

namespace App.Api.Shared.Infrastructure;

public static class FunctionConfiguration
{
  private static IConfiguration? _configuration;

  public static IConfiguration Get() => _configuration ??= new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: true)
      .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
      .AddEnvironmentVariables()
      .Build();
}
