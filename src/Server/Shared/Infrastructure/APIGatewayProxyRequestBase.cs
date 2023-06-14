using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;

namespace App.Api.Shared.Infrastructure;

public abstract class APIGatewayProxyRequestBase : FunctionBase
{
  protected abstract Task<APIGatewayHttpApiV2ProxyResponse> Handler(APIGatewayProxyRequest apiGatewayProxyRequest);

  [Logging(LogEvent = true)]
  public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
    APIGatewayProxyRequest apiGatewayProxyRequest,
    ILambdaContext context)
  {
    APIGatewayProxyRequestAccessor.Current = apiGatewayProxyRequest;
    AppendLookup(apiGatewayProxyRequest);

    return await Handler(apiGatewayProxyRequest);
  }
}
