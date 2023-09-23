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

namespace App.Api.ListAccounts;

public class Function : APIGatewayProxyRequestBase
{
  protected override void ConfigureServices(IServiceCollection services)
  {
    services.AddMediatR(Assembly.GetCallingAssembly());
    services.AddTransient<IResponse<List<AccountDto>>, Response<List<AccountDto>>>();
    services.AddTransient<IRequestHandler<ListAccountsQuery.Query, IResponse<List<AccountDto>>>, ListAccountsQuery.QueryHandler>();
    services.AddTransient<IValidator<ListAccountsQuery.Query>, ListAccountsQuery.QueryValidator>();
  }

  protected override async Task<APIGatewayHttpApiV2ProxyResponse> Handler(APIGatewayProxyRequest apiGatewayProxyRequest)
    => await Respond(new ListAccountsQuery.Query());
}
