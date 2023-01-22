using App.Login.Features.User;
using App.Login.Features.UserInfo;
using App.Login.Tests.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace App.Login.Tests.Features.UserInfo;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class UserInfoQueryTests : TestBase
{
  [Fact]
  public async Task Should_ReturnPrincipal_When_UserIsAuthenticated() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var email = "valid@gomsle.com";
      var phoneNumber = "0731234567";
      var password = "itsaseasyas123";
      var user = new DynamoDbUser
      {
        Email = email,
        UserName = email,
        EmailConfirmed = true,
        TwoFactorEnabled = false,
        PhoneNumber = phoneNumber,
        PhoneNumberConfirmed = true,
      };
      await userManager.CreateAsync(user, password);
      await mediator.Send(new LoginCommand.Command
      {
        Email = email,
        Password = password,
        RememberMe = false,
      });
      var httpContext = GetMock<HttpContext>();
      var featureCollection = new FeatureCollection();
      featureCollection.Set(new OpenIddictServerAspNetCoreFeature
      {
        Transaction = new OpenIddictServerTransaction
        {
          Request = new OpenIddictRequest
          {
            Scope = "test",
          },
        },
      });
      httpContext!.Setup(x => x.Features).Returns(featureCollection);
      var command = new UserInfoQuery.Query();

      // Act
      var result = await mediator.Send(command);

      // Assert
      Assert.True(result.IsValid);
      Assert.Contains(result.Result!, x =>
        x.Key == Claims.PhoneNumber && x.Value.ToString() == phoneNumber);
      Assert.Contains(result.Result!, x =>
        x.Key == Claims.Email && x.Value.ToString() == email);
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
      var command = new UserInfoQuery.Query();

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == Errors.InvalidToken);
    });
}
