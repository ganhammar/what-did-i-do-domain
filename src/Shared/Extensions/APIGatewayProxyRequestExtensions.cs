using Amazon.Lambda.APIGatewayEvents;

namespace App.Api.Shared.Extensions;

public static class APIGatewayProxyRequestExtensions
{
  public static string? GetSubject(this APIGatewayProxyRequest apiGatewayProxyRequest)
  {
    if (apiGatewayProxyRequest.RequestContext.Authorizer.TryGetValue("sub", out var subject))
    {
      return subject.ToString();
    }

    return default;
  }

  public static string? GetEmail(this APIGatewayProxyRequest apiGatewayProxyRequest)
  {
    if (apiGatewayProxyRequest.RequestContext.Authorizer.TryGetValue("email", out var email))
    {
      return email.ToString();
    }

    return default;
  }
}
