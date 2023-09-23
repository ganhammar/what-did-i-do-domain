using App.Login.Features.User;
using App.Login.Tests.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace App.Login.Tests.Features.User;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class LoginCommandTests : TestBase
{
  [Fact]
  public async Task Should_LoginUser_When_CommandIsValid() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var email = "valid@wdid.fyi";
      var password = "itsaseasyas123";
      var user = new DynamoDbUser
      {
        Email = email,
        UserName = email,
        EmailConfirmed = true,
        TwoFactorEnabled = false,
        LockoutEnabled = false,
      };
      await userManager.CreateAsync(user, password);

      var command = new LoginCommand.Command
      {
        Email = email,
        Password = password,
        RememberMe = false,
      };

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.True(response.IsValid);
      Assert.True(response.Result!.Succeeded);
    });

  [Fact]
  public async Task Should_NotBeValid_When_EmailAndUserNameIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var command = new LoginCommand.Command
      {
        Password = "itsnotaseasyas123",
      };
      var validator = new LoginCommand.CommandValidator(userManager);

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "Email");
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "UserName");
    });

  [Fact]
  public async Task Should_NotBeValid_When_PasswordIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var command = new LoginCommand.Command
      {
        Email = "valid@wdid.fyi",
      };
      var validator = new LoginCommand.CommandValidator(userManager);

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "Password");
    });

  [Fact]
  public async Task Should_NotBeValid_When_BothUserNameAndEmailIsSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var command = new LoginCommand.Command
      {
        Email = "valid@wdid.fyi",
        UserName = "valid",
        Password = "itsnotaseasyas123",
      };
      var validator = new LoginCommand.CommandValidator(userManager);

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "EmptyValidator" && error.PropertyName == "UserName");
    });
}
