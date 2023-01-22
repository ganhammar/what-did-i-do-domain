using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using AWS.Lambda.Powertools.Logging;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Api.Shared.Infrastructure;

public abstract class FunctionBase
{
  private readonly IServiceProvider _serviceProvider;
  private readonly APIGatewayHttpApiV2ProxyResponse _noBodyResponse = new APIGatewayHttpApiV2ProxyResponse
  {
    Body = JsonSerializer.Serialize(new[]
    {
      new ValidationFailure("Body", "Invalid request"),
    }),
    StatusCode = (int)HttpStatusCode.BadRequest,
  };

  public FunctionBase(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public async Task<APIGatewayHttpApiV2ProxyResponse> Respond<T>(IRequest<IResponse<T>>? request)
  {
    if (request == default)
    {
      return _noBodyResponse;
    }

    var mediator = _serviceProvider.GetRequiredService<IMediator>();
    var response = await mediator.Send(request);

    if (response.IsValid)
    {
      return new APIGatewayHttpApiV2ProxyResponse
      {
        StatusCode = (int)HttpStatusCode.OK,
        Body = JsonSerializer.Serialize(response.Result),
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };
    }

    return HandleErrorResponse(response);
  }

  public async Task<APIGatewayHttpApiV2ProxyResponse> Respond(IRequest<IResponse>? request)
  {
    if (request == default)
    {
      return _noBodyResponse;
    }

    var mediator = _serviceProvider.GetRequiredService<IMediator>();
    var response = await mediator.Send(request);

    if (response.IsValid)
    {
      return new APIGatewayHttpApiV2ProxyResponse
      {
        StatusCode = (int)HttpStatusCode.NoContent,
      };
    }

    return HandleErrorResponse(response);
  }

  public void AppendLookup(APIGatewayProxyRequest apiGatewayProxyRequest)
  {
    var requestContextRequestId = apiGatewayProxyRequest.RequestContext.RequestId;
    var lookupInfo = new Dictionary<string, object>()
    {
      { "LookupInfo", new Dictionary<string, object>{{ "LookupId", requestContextRequestId }} },
    };
    Logger.AppendKeys(lookupInfo);
  }

  private APIGatewayHttpApiV2ProxyResponse HandleErrorResponse(IResponse response)
    => new APIGatewayHttpApiV2ProxyResponse
    {
      StatusCode = (int)HttpStatusCode.BadRequest,
      Body = JsonSerializer.Serialize(response.Errors),
      Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    };
}
