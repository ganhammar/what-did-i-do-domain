using System.Reflection;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using AWS.Lambda.Powertools.Logging;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace App.Api.ListEvents;

public class Function : FunctionBase
{
  protected override void ConfigureServices(IServiceCollection services)
  {
    services.AddMediatR(Assembly.GetCallingAssembly());
    services.AddTransient<IResponse<List<EventDto>>, Response<List<EventDto>>>();
    services.AddTransient<IRequestHandler<ListEventsCommand.Command, IResponse<List<EventDto>>>, ListEventsCommand.CommandHandler>();
    services.AddTransient<IValidator<ListEventsCommand.Command>, ListEventsCommand.CommandValidator>();
  }

  [Logging(LogEvent = true)]
  public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
    APIGatewayProxyRequest apiGatewayProxyRequest,
    ILambdaContext context)
  {
    AppendLookup(apiGatewayProxyRequest);

    return await Respond(JsonSerializer.Deserialize<ListEventsCommand.Command>(
      apiGatewayProxyRequest.Body,
      new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
      }));
  }
}
