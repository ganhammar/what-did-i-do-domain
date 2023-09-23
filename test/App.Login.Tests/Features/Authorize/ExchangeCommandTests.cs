using System.Security.Claims;
using App.Login.Features.Authorization;
using App.Login.Tests.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
  public async Task Should_ReturnPrincipal_When_ClientCredentialsRequestIsValid() =>
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
  public async Task Should_ReturnPrincipal_When_AuthorizationCodeRequestIsValid() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var user = await CreateAndLoginValidUser(services);
      var authenticationService = GetMock<IAuthenticationService>();
      authenticationService!.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
        .Returns(Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
          new Claim(Claims.Subject, user.Id),
        })), default, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))));

      var applicationManager = services.GetRequiredService<IOpenIddictApplicationManager>();
      var clientId = Guid.NewGuid().ToString();
      var application = new OpenIddictDynamoDbApplication
      {
        ClientId = clientId,
        ConsentType = ConsentTypes.Explicit,
        Type = ClientTypes.Public,
      };
      await applicationManager.CreateAsync(application);
      var httpContext = GetMock<HttpContext>();
      var featureCollection = new FeatureCollection();
      featureCollection.Set(new OpenIddictServerAspNetCoreFeature
      {
        Transaction = new OpenIddictServerTransaction
        {
          Request = new OpenIddictRequest
          {
            ClientId = clientId,
            GrantType = GrantTypes.AuthorizationCode,
            Scope = "test",
            ResponseType = ResponseTypes.Code,
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

  [Fact]
  public async Task Should_NotBeValid_When_AuthorizationCodeRequestHasNoUser() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var authenticationService = GetMock<IAuthenticationService>();
      authenticationService!.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
        .Returns(Task.FromResult(AuthenticateResult.Fail(new Exception("Fails"))));

      var applicationManager = services.GetRequiredService<IOpenIddictApplicationManager>();
      var clientId = Guid.NewGuid().ToString();
      var application = new OpenIddictDynamoDbApplication
      {
        ClientId = clientId,
        ConsentType = ConsentTypes.Explicit,
        Type = ClientTypes.Public,
      };
      await applicationManager.CreateAsync(application);
      var httpContext = GetMock<HttpContext>();
      var featureCollection = new FeatureCollection();
      featureCollection.Set(new OpenIddictServerAspNetCoreFeature
      {
        Transaction = new OpenIddictServerTransaction
        {
          Request = new OpenIddictRequest
          {
            ClientId = clientId,
            GrantType = GrantTypes.AuthorizationCode,
            Scope = "test",
            ResponseType = ResponseTypes.Code,
          },
        },
      });
      httpContext!.Setup(x => x.Features).Returns(featureCollection);
      var command = new ExchangeCommand.Command();

      // Act
      var result = await mediator.Send(command);

      // Assert
      Assert.False(result.IsValid);
      Assert.Contains(result.Errors, error =>
        error.ErrorCode == Errors.InvalidGrant);
    });

  [Fact]
  public async Task Should_NotBeValid_When_UserCantLogin() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var user = await CreateAndLoginValidUser(services);
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      await userManager.SetLockoutEnabledAsync(user, true);
      await userManager.UpdateAsync(user);

      var authenticationService = GetMock<IAuthenticationService>();
      authenticationService!.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
        .Returns(Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
          new Claim(Claims.Subject, user.Id),
        })), default, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))));

      var applicationManager = services.GetRequiredService<IOpenIddictApplicationManager>();
      var clientId = Guid.NewGuid().ToString();
      var application = new OpenIddictDynamoDbApplication
      {
        ClientId = clientId,
        ConsentType = ConsentTypes.Explicit,
        Type = ClientTypes.Public,
      };
      await applicationManager.CreateAsync(application);
      var httpContext = GetMock<HttpContext>();
      var featureCollection = new FeatureCollection();
      featureCollection.Set(new OpenIddictServerAspNetCoreFeature
      {
        Transaction = new OpenIddictServerTransaction
        {
          Request = new OpenIddictRequest
          {
            ClientId = clientId,
            GrantType = GrantTypes.AuthorizationCode,
            Scope = "test",
            ResponseType = ResponseTypes.Code,
          },
        },
      });
      httpContext!.Setup(x => x.Features).Returns(featureCollection);
      var command = new ExchangeCommand.Command();

      // Act
      var result = await mediator.Send(command);

      // Assert
      Assert.False(result.IsValid);
      Assert.Contains(result.Errors, error =>
        error.ErrorCode == Errors.InvalidGrant);
    });
}
