using System.Reflection;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using App.Api.Shared.Infrastructure;
using AWS.Lambda.Powertools.Logging;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace App.Api.DeleteEvent;

public class Function : FunctionBase
{
  protected override void ConfigureServices(IServiceCollection services)
  {
    services.AddMediatR(Assembly.GetCallingAssembly());
    services.AddTransient<IResponse, Response>();
    services.AddTransient<IRequestHandler<DeleteEventCommand.Command, IResponse>, DeleteEventCommand.CommandHandler>();
    services.AddTransient<IValidator<DeleteEventCommand.Command>, DeleteEventCommand.CommandValidator>();
  }

  [Logging(LogEvent = true)]
  public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
    APIGatewayProxyRequest apiGatewayProxyRequest,
    ILambdaContext context)
  {
    AppendLookup(apiGatewayProxyRequest);

    return await Respond(JsonSerializer.Deserialize<DeleteEventCommand.Command>(
      apiGatewayProxyRequest.Body,
      new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
      }));
  }
}
