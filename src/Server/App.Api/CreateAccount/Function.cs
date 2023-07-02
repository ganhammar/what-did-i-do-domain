using System.Reflection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace App.Api.CreateAccount;

public class Function : APIGatewayProxyRequestBase
{
  protected override void ConfigureServices(IServiceCollection services)
  {
    services.AddMediatR(Assembly.GetCallingAssembly());
    services.AddTransient<IResponse<AccountDto>, Response<AccountDto>>();
    services.AddTransient<IRequestHandler<CreateAccountCommand.Command, IResponse<AccountDto>>, CreateAccountCommand.CommandHandler>();
    services.AddTransient<IValidator<CreateAccountCommand.Command>, CreateAccountCommand.CommandValidator>();
  }

  protected override async Task<APIGatewayHttpApiV2ProxyResponse> Handler(
    APIGatewayProxyRequest apiGatewayProxyRequest)
  {
    return await Respond(TryDeserialize<CreateAccountCommand.Command>(apiGatewayProxyRequest));
  }
}
