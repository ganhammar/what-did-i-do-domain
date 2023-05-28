using System.Reflection;
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

    var queryStringParameters = new Dictionary<string, string>(
      apiGatewayProxyRequest.QueryStringParameters ?? new Dictionary<string, string>(),
      StringComparer.OrdinalIgnoreCase);

    queryStringParameters.TryGetValue("accountid", out var accountId);
    queryStringParameters.TryGetValue("fromdate", out var fromDateRaw);
    queryStringParameters.TryGetValue("todate", out var toDateRaw);

    var fromDate = TryParseDateTime(fromDateRaw);
    var toDate = TryParseDateTime(toDateRaw);

    return await Respond(new ListEventsCommand.Command
    {
      AccountId = accountId,
      FromDate = fromDate,
      ToDate = toDate,
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
