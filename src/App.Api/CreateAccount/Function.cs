using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using App.Api.Shared.Infrastructure;
using AWS.Lambda.Powertools.Logging;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace App.Api.CreateAccount;

public class Function : FunctionBase
{
  private static readonly IServiceProvider _serviceProvider = Startup
    .ConfigureServices()
    .BuildServiceProvider();

  public Function() : base(_serviceProvider) { }

  [Logging(LogEvent = true)]
  public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
    APIGatewayProxyRequest apiGatewayProxyRequest,
    ILambdaContext context)
  {
    AppendLookup(apiGatewayProxyRequest);

    return await Respond(JsonSerializer.Deserialize<CreateAccountCommand.Command>(
      apiGatewayProxyRequest.Body,
      new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
      }));
  }
}
