using App.Login.Features.UserInfo;
using App.Login.Tests.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;

namespace App.Login.Tests.Features.UserInfo;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class UserInfoControllerTests : TestBase
{
  private Func<IServiceProvider, object[]> ConfigureController = (services) =>
  {
    var mediator = services.GetRequiredService<IMediator>();

    return new object[] { mediator };
  };

  [Fact]
  public async Task Should_ReturnOk_When_UserInfoQueryIsValid() => await ControllerTest<UserInfoController>(
      // Arrange
      ConfigureController,
      // Act & Assert
      async (controller, services) =>
      {
        // Arrange
        await CreateAndLoginValidUser(services);

        // Act
        var result = await controller.UserInfo(new());

        // Assert
        Assert.NotNull(result);

        var okObjectResult = result as OkObjectResult;
        Assert.NotNull(okObjectResult);
      });

  [Fact]
  public async Task Should_ReturnChallenge_When_UserIsntAuthenticated() => await ControllerTest<UserInfoController>(
      // Arrange
      ConfigureController,
      // Act & Assert
      async (controller, services) =>
      {
        // Arrange
        var mediator = services.GetRequiredService<IMediator>();
        var httpContext = GetMock<HttpContext>();
        var featureCollection = new FeatureCollection();
        featureCollection.Set(new OpenIddictServerAspNetCoreFeature
        {
          Transaction = new OpenIddictServerTransaction
          {
            Request = new OpenIddictRequest(),
          },
        });
        httpContext!.Setup(x => x.Features).Returns(featureCollection);

        // Act
        var result = await controller.UserInfo(new());

        // Assert
        Assert.NotNull(result);

        var challengeResult = result as ChallengeResult;
        Assert.NotNull(challengeResult);
      });
}
