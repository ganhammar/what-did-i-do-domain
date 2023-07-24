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

namespace App.Api.ListTags;

public class Function : APIGatewayProxyRequestBase
{
  protected override void ConfigureServices(IServiceCollection services)
  {
    services.AddMediatR(Assembly.GetCallingAssembly());
    services.AddTransient<IResponse<List<TagDto>>, Response<List<TagDto>>>();
    services.AddTransient<IRequestHandler<ListTagsQuery.Query, IResponse<List<TagDto>>>, ListTagsQuery.QueryHandler>();
    services.AddTransient<IValidator<ListTagsQuery.Query>, ListTagsQuery.QueryValidator>();
  }

  protected override async Task<APIGatewayHttpApiV2ProxyResponse> Handler(
    APIGatewayProxyRequest apiGatewayProxyRequest)
  {
    var queryStringParameters = new Dictionary<string, string>(
      apiGatewayProxyRequest.QueryStringParameters ?? new Dictionary<string, string>(),
      StringComparer.OrdinalIgnoreCase);

    queryStringParameters.TryGetValue("accountid", out var accountId);

    return await Respond(new ListTagsQuery.Query
    {
      AccountId = accountId,
    });
  }
}
