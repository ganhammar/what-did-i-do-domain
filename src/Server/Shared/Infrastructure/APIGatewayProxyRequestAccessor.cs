using Amazon.Lambda.APIGatewayEvents;

namespace App.Api.Shared.Infrastructure;

public static class APIGatewayProxyRequestAccessor
{
  public static APIGatewayProxyRequest? Current { get; set; }
}
