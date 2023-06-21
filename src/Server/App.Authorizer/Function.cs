using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using App.Api.Shared.Infrastructure;
using AWS.Lambda.Powertools.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: LambdaSerializer(typeof(CamelCaseLambdaJsonSerializer))]

namespace App.Authorizer;

public class Function : FunctionBase
{
  protected override void ConfigureServices(IServiceCollection services)
  {
    services.Configure<AuthorizationOptions>(Configuration.GetSection(nameof(AuthorizationOptions)));
    services.AddMemoryCache();
    services.AddHttpClient<TokenClient>();
  }

  [Logging(LogEvent = true)]
  public async Task<APIGatewayCustomAuthorizerResponse> FunctionHandler(
    APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
  {
    AppendLookup(request.RequestContext.RequestId);

    var headers = new Dictionary<string, string>(request.Headers, StringComparer.OrdinalIgnoreCase);

    headers.TryGetValue("authorization", out var token);

    var options = ServiceProvider.GetRequiredService<IOptionsMonitor<AuthorizationOptions>>();
    var tokenClient = ServiceProvider.GetRequiredService<TokenClient>();

    if (token == default)
    {
      throw new Exception("Unauthorized");
    }

    var result = await tokenClient.Validate(options.CurrentValue, token);

    return new()
    {
      PrincipalID = "user",
      PolicyDocument = new()
      {
        Version = "2012-10-17",
        Statement = new()
        {
          new()
          {
            Effect = "Allow",
            Resource = new() { request.MethodArn },
            Action = new() { "execute-api:Invoke" },
          },
        },
      },
      Context = new()
      {
        { "scope", result.Scope },
        { "sub", result.Subject },
      },
    };
  }
}
