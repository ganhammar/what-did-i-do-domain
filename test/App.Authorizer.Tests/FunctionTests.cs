using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace App.Authorizer.Test;

public class FunctionTests
{
  [Fact]
  public async Task Should_Allow_When_TokenIsValid()
  {
    var request = new APIGatewayCustomAuthorizerRequest
    {
      Headers = new Dictionary<string, string>
      {
        { "authorization", "1234" },
      },
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var context = new TestLambdaContext();
    var function = GetFunction(true);

    var result = await function.FunctionHandler(request, context);

    Assert.Equal("Allow", result.PolicyDocument.Statement.First().Effect);
  }

  [Fact]
  public async Task Should_Throw_When_TokenIsNotSet()
  {
    var request = new APIGatewayCustomAuthorizerRequest
    {
      Headers = new Dictionary<string, string>(),
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var context = new TestLambdaContext();
    var function = GetFunction(true);

    await Assert.ThrowsAsync<UnauthorizedAccessException>(
      async () => await function.FunctionHandler(request, context));
  }

  [Fact]
  public async Task Should_Throw_When_TokenIsNotActive()
  {
    var request = new APIGatewayCustomAuthorizerRequest
    {
      Headers = new Dictionary<string, string>
      {
        { "authorization", "1234" },
      },
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var context = new TestLambdaContext();
    var function = GetFunction(false);

    await Assert.ThrowsAsync<UnauthorizedAccessException>(
      async () => await function.FunctionHandler(request, context));
  }

  private Function GetFunction(bool isActive)
  {
    var function = new Mock<Function>();
    function
      .Protected()
      .Setup("ConfigureServices", ItExpr.IsAny<IServiceCollection>())
      .Callback((IServiceCollection services) =>
      {
        var mockedTokenClient = new Mock<ITokenClient>();
        mockedTokenClient
          .Setup(x => x.Validate(It.IsAny<AuthorizationOptions>(), It.IsAny<string>()))
          .Returns(Task.FromResult(new IntrospectionResult
          {
            Active = isActive,
            Scope = "test",
            Audience = "test",
            Subject = "123",
            Email = "test@wdid.fyi",
            TokenUsage = "access_token",
          }));
        services.AddSingleton<ITokenClient>(mockedTokenClient.Object);

        var mockedOptions = new Mock<IOptionsMonitor<AuthorizationOptions>>();
        mockedOptions.Setup(x => x.CurrentValue).Returns(new AuthorizationOptions());
        services.AddSingleton<IOptionsMonitor<AuthorizationOptions>>(mockedOptions.Object);
      });

    return function.Object;
  }
}
