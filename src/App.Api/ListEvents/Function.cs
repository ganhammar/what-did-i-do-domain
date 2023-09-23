using System.Reflection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using App.Api.Shared.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(CamelCaseLambdaJsonSerializer))]

namespace App.Api.ListEvents;

public class Function : APIGatewayProxyRequestBase
{
  protected override void ConfigureServices(IServiceCollection services)
  {
    services.AddMediatR(Assembly.GetCallingAssembly());
    services.AddTransient<IResponse<Result>, Response<Result>>();
    services.AddTransient<IRequestHandler<ListEventsQuery.Query, IResponse<Result>>, ListEventsQuery.QueryHandler>();
    services.AddTransient<IValidator<ListEventsQuery.Query>, ListEventsQuery.QueryValidator>();
  }

  protected override async Task<APIGatewayHttpApiV2ProxyResponse> Handler(
    APIGatewayProxyRequest apiGatewayProxyRequest)
  {
    var queryStringParameters = new Dictionary<string, string>(
      apiGatewayProxyRequest.QueryStringParameters ?? new Dictionary<string, string>(),
      StringComparer.OrdinalIgnoreCase);

    queryStringParameters.TryGetValue("accountid", out var accountId);
    queryStringParameters.TryGetValue("fromdate", out var fromDateRaw);
    queryStringParameters.TryGetValue("todate", out var toDateRaw);
    queryStringParameters.TryGetValue("limit", out var limitRaw);
    queryStringParameters.TryGetValue("tag", out var tag);
    queryStringParameters.TryGetValue("paginationtoken", out var paginationToken);

    var fromDate = TryParseDateTime(fromDateRaw);
    var toDate = TryParseDateTime(toDateRaw);
    int.TryParse(limitRaw, out var limit);

    return await Respond(new ListEventsQuery.Query
    {
      AccountId = accountId,
      FromDate = fromDate,
      ToDate = toDate,
      Limit = limit,
      Tag = tag,
      PaginationToken = paginationToken,
    });
  }

  private DateTime? TryParseDateTime(string? date)
  {
    if (DateTime.TryParse(date, out var parseDate))
    {
      return parseDate;
    }

    return default;
  }
}
