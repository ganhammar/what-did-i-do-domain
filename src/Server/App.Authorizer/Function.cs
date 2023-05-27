using System.IdentityModel.Tokens.Jwt;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using App.Api.Shared.Infrastructure;
using AWS.Lambda.Powertools.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

[assembly: LambdaSerializer(typeof(CamelCaseLambdaJsonSerializer))]

namespace App.Authorizer.Authorizer;

public class Function : FunctionBase
{
  protected override void ConfigureServices(IServiceCollection services)
  {
    services.Configure<AuthorizationOptions>(Configuration.GetSection(nameof(AuthorizationOptions)));
  }

  [Logging(LogEvent = true)]
  public async Task<APIGatewayCustomAuthorizerResponse> FunctionHandler(
    APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
  {
    AppendLookup(request.RequestContext.RequestId);

    var options = ServiceProvider.GetRequiredService<IOptionsMonitor<AuthorizationOptions>>();
    var result = await ValidateTokenAsync(
      options.CurrentValue.Issuer, request.AuthorizationToken);

    return new()
    {
      PrincipalID = "user",
      PolicyDocument = new()
      {
        Version = "2012-10-17",
        Statement = new()
        {
          new()
          {
            Effect = result.IsValid ? "Allow" : "Deny",
            Resource = new() { request.MethodArn },
            Action = new() { "execute-api:Invoke" },
          },
        },
      },
    };
  }

  private async Task<TokenValidationResult> ValidateTokenAsync(string? issuer, string? token)
  {
    ArgumentNullException.ThrowIfNull(issuer);

    var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
      $"{issuer}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());

    var config = await configurationManager.GetConfigurationAsync();

    token = token?.Replace("Bearer ", "");

    var tokenHandler = new JwtSecurityTokenHandler();
    var result = await tokenHandler.ValidateTokenAsync(token, new()
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKeys = config.SigningKeys,
      ValidIssuer = issuer,
      ValidateAudience = false,
    });

    if (result.IsValid)
    {
      Logger.LogInformation($"Token successfully validated");
    }
    else
    {
      Logger.LogWarning(result.Exception, $"Could not validate token for issuer {issuer}");
    }

    return result;
  }
}
