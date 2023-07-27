﻿using System.Reflection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
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
    services.AddTransient<IResponse<List<EventDto>>, Response<List<EventDto>>>();
    services.AddTransient<IRequestHandler<ListEventsQuery.Query, IResponse<List<EventDto>>>, ListEventsQuery.QueryHandler>();
    services.AddTransient<IValidator<ListEventsQuery.Query>, ListEventsQuery.QueryValidator>();
  }

  protected override async Task<APIGatewayHttpApiV2ProxyResponse> Handler(
    APIGatewayProxyRequest apiGatewayProxyRequest)
  {
    var queryStringParameters = new Dictionary<string, IList<string>>(
      apiGatewayProxyRequest.MultiValueQueryStringParameters ?? new Dictionary<string, IList<string>>(),
      StringComparer.OrdinalIgnoreCase);

    queryStringParameters.TryGetValue("accountid", out var accountId);
    queryStringParameters.TryGetValue("fromdate", out var fromDateRaw);
    queryStringParameters.TryGetValue("todate", out var toDateRaw);
    queryStringParameters.TryGetValue("limit", out var limitRaw);
    queryStringParameters.TryGetValue("tags", out var tags);

    var fromDate = TryParseDateTime(fromDateRaw?.First());
    var toDate = TryParseDateTime(toDateRaw?.First());
    int.TryParse(limitRaw?.First(), out var limit);

    return await Respond(new ListEventsQuery.Query
    {
      AccountId = accountId?.First(),
      FromDate = fromDate,
      ToDate = toDate,
      Limit = limit,
      Tags = tags,
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
