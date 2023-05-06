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

namespace App.Api.CreateAccount;

public class Function : FunctionBase
{
  protected override void ConfigureServices(IServiceCollection services)
  {
    services.AddMediatR(Assembly.GetCallingAssembly());
    services.AddTransient<IResponse<AccountDto>, Response<AccountDto>>();
    services.AddTransient<IRequestHandler<CreateAccountCommand.Command, IResponse<AccountDto>>, CreateAccountCommand.CommandHandler>();
    services.AddTransient<IValidator<CreateAccountCommand.Command>, CreateAccountCommand.CommandValidator>();
  }

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
