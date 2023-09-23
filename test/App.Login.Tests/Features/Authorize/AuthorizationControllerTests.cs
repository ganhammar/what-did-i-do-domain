using App.Login.Features.Authorization;
using App.Login.Tests.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.AmazonDynamoDB;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace App.Login.Tests.Features.Authorize;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class AuthorizationControllerTests : TestBase
{
  private Func<IServiceProvider, object[]> ConfigureController = (services) =>
  {
    var mediator = services.GetRequiredService<IMediator>();

    return new object[] { mediator };
  };

  [Fact]
  public async Task Should_ReturnSignInResult_When_AuthorizationRequestIsValid() => await ControllerTest<AuthorizationController>(
      // Arrange
      ConfigureController,
      // Act & Assert
      async (controller, services) =>
      {
        // Arrange
        await CreateAndLoginValidUser(services);

        // Act
        var result = await controller.Authorize(new());

        // Assert
        Assert.NotNull(result);

        var signInResult = result as SignInResult;
        Assert.NotNull(signInResult);
      });

  [Fact]
  public async Task Should_ReturnChallenge_When_UserIsntAuthenticated() => await ControllerTest<AuthorizationController>(
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
        var result = await controller.Authorize(new());

        // Assert
        Assert.NotNull(result);

        var challengeResult = result as ChallengeResult;
        Assert.NotNull(challengeResult);
      });

  [Fact]
  public async Task Should_ReturnForbid_When_AuthorizeCommandIsntValid() => await ControllerTest<AuthorizationController>(
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
            Request = new OpenIddictRequest
            {
              Prompt = Prompts.None,
            },
          },
        });
        httpContext!.Setup(x => x.Features).Returns(featureCollection);

        // Act
        var result = await controller.Authorize(new());

        // Assert
        Assert.NotNull(result);

        var forbidResult = result as ForbidResult;
        Assert.NotNull(forbidResult);
      });

  [Fact]
  public async Task Should_Return_When_LoggingOut() => await ControllerTest<AuthorizationController>(
      // Arrange
      ConfigureController,
      // Act & Assert
      async (controller, services) =>
      {
        // Act
        var result = await controller.LogoutCommand(new());

        // Assert
        Assert.NotNull(result);

        var signOutResult = result as SignOutResult;
        Assert.NotNull(signOutResult);
      });

  [Fact]
  public async Task Should_ReturnSignInResult_When_ExchangeCommandRequestIsValid() => await ControllerTest<AuthorizationController>(
      // Arrange
      ConfigureController,
      // Act & Assert
      async (controller, services) =>
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

        // Act
        var result = await controller.ExchangeCommand(new());

        // Assert
        Assert.NotNull(result);

        var signInResult = result as SignInResult;
        Assert.NotNull(signInResult);
      });

  [Fact]
  public async Task Should_ReturnForbid_When_ExchangeCommandRequestIsntValid() => await ControllerTest<AuthorizationController>(
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
            Request = new OpenIddictRequest
            {
              Prompt = Prompts.None,
            },
          },
        });
        httpContext!.Setup(x => x.Features).Returns(featureCollection);

        // Act
        var result = await controller.ExchangeCommand(new());

        // Assert
        Assert.NotNull(result);

        var forbidResult = result as ForbidResult;
        Assert.NotNull(forbidResult);
      });
}
