using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using FluentValidation.Results;

namespace App.Api.Shared.Infrastructure;

public abstract class APIGatewayProxyRequestBase : FunctionBase
{
  protected abstract Task<APIGatewayHttpApiV2ProxyResponse> Handler(APIGatewayProxyRequest apiGatewayProxyRequest);

  protected List<string> RequiredScopes = new();

  [Logging(LogEvent = true)]
  public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
    APIGatewayProxyRequest apiGatewayProxyRequest,
    ILambdaContext context)
  {
    APIGatewayProxyRequestAccessor.Current = apiGatewayProxyRequest;
    AppendLookup(apiGatewayProxyRequest);

    if (this.RequiredScopes.Any())
    {
      if (apiGatewayProxyRequest.RequestContext.Authorizer.TryGetValue("scope", out var scopes) == false)
      {
        return HandleUnauthorizedResponse("User not authenticated");
      }

      if (scopes == null || this.RequiredScopes.Except((scopes as string)!.Split(" ")).Any() == true)
      {
        return HandleUnauthorizedResponse("User not authorized to perform this request");
      }
    }

    return await Handler(apiGatewayProxyRequest);
  }

  private APIGatewayHttpApiV2ProxyResponse HandleUnauthorizedResponse(string message)
    => new APIGatewayHttpApiV2ProxyResponse
    {
      StatusCode = (int)HttpStatusCode.Unauthorized,
      Body = JsonSerializer.Serialize(new List<ValidationFailure>
      {
        new("Body", message)
        {
          ErrorCode = "UnauthorizedRequest",
        },
      }),
      Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    };
}
