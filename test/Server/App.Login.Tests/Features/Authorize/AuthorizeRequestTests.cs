using App.Login.Features.Authorization;
using App.Login.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace App.Login.Tests.Features.Authorize;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class AuthorizeCommandTests : TestBase
{
  [Fact]
  public async Task Should_ReturnPrincipal_When_UserIsAuthenticated() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      await CreateAndLoginValidUser(services);
      var command = new AuthorizeCommand.Command();

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
        var command = new AuthorizeCommand.Command();

        // Act
        var response = await mediator.Send(command);

        // Assert
        Assert.False(response.IsValid);
        Assert.Contains(response.Errors, error =>
              error.ErrorCode == "NoAuthorizationRequestInProgress");
      });

  [Fact]
  public async Task Should_RequireLogin_When_UserIsntAuthenticated() =>
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
              Prompt = Prompts.None,
            },
          },
        });
        httpContext!.Setup(x => x.Features).Returns(featureCollection);
        var command = new AuthorizeCommand.Command();

        // Act
        var response = await mediator.Send(command);

        // Assert
        Assert.False(response.IsValid);
        Assert.Contains(response.Errors, error =>
              error.ErrorCode == Errors.LoginRequired);
      });
}
