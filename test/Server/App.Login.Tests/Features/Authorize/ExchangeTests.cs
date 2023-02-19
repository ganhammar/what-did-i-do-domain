using App.Login.Features.Authorization;
using App.Login.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.AmazonDynamoDB;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace App.Login.Tests.Features.Authorize;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class ExchangeCommandTests : TestBase
{
  [Fact]
  public async Task Should_ReturnPrincipal_When_RequestIsValid() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var applicationManager = services.GetRequiredService<IOpenIddictApplicationManager>();
      var clientId = Guid.NewGuid().ToString();
      var clientSecret = Guid.NewGuid().ToString();
      var application = new OpenIddictDynamoDbApplication
      {
        ClientId = clientId,
      };
      await applicationManager.CreateAsync(application, clientSecret);
      var httpContext = GetMock<HttpContext>();
      var featureCollection = new FeatureCollection();
      featureCollection.Set(new OpenIddictServerAspNetCoreFeature
      {
        Transaction = new OpenIddictServerTransaction
        {
          Request = new OpenIddictRequest
          {
            ClientId = clientId,
            ClientSecret = clientSecret,
            GrantType = GrantTypes.ClientCredentials,
            Scope = "test",
          },
        },
      });
      httpContext!.Setup(x => x.Features).Returns(featureCollection);
      var command = new ExchangeCommand.Command();

      // Act
      var result = await mediator.Send(command);

      // Assert
      Assert.True(result.IsValid);
      Assert.NotNull(result.Result!.Identity);
      Assert.True(result.Result!.Identity!.IsAuthenticated);
    });

  [Fact]
  public async Task Should_NotBeValid_When_AuthorizationRequestIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var httpContext = GetMock<HttpContext>();
      var featureCollection = new FeatureCollection();
      featureCollection.Set(new OpenIddictServerAspNetCoreFeature
      {
        Transaction = new OpenIddictServerTransaction(),
      });
      httpContext!.Setup(x => x.Features).Returns(featureCollection);
      var command = new ExchangeCommand.Command();

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
            error.ErrorCode == "NoAuthorizationRequestInProgress");
    });

  [Fact]
  public async Task Should_NotBeValid_When_GrantTypeIsNotSupported() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var httpContext = GetMock<HttpContext>();
      var featureCollection = new FeatureCollection();
      featureCollection.Set(new OpenIddictServerAspNetCoreFeature
      {
        Transaction = new OpenIddictServerTransaction
        {
          Request = new OpenIddictRequest
          {
            ClientId = "test",
            ClientSecret = "test",
            GrantType = GrantTypes.Implicit,
            Scope = "test",
          },
        },
      });
      httpContext!.Setup(x => x.Features).Returns(featureCollection);
      var command = new ExchangeCommand.Command();

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == Errors.UnsupportedGrantType);
    });
}
