using System.Reflection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(CamelCaseLambdaJsonSerializer))]

namespace App.Api.EditEvent;

public class Function : APIGatewayProxyRequestBase
{
  protected override void ConfigureServices(IServiceCollection services)
  {
    services.AddMediatR(Assembly.GetCallingAssembly());
    services.AddTransient<IResponse<EventDto>, Response<EventDto>>();
    services.AddTransient<IRequestHandler<EditEventCommand.Command, IResponse<EventDto>>, EditEventCommand.CommandHandler>();
    services.AddTransient<IValidator<EditEventCommand.Command>, EditEventCommand.CommandValidator>();
  }

  protected override async Task<APIGatewayHttpApiV2ProxyResponse> Handler(
    APIGatewayProxyRequest apiGatewayProxyRequest)
  {
    return await Respond(TryDeserialize<EditEventCommand.Command>(apiGatewayProxyRequest));
  }
}
