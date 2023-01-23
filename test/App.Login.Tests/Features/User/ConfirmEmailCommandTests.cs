using App.Login.Features.User;
using App.Login.Tests.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace App.Login.Tests.Features.User;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class ConfirmEmailCommandTests : TestBase
{
  [Fact]
  public async Task Should_ConfirmEmailCommand_When_CommandIsValid() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var user = new DynamoDbUser
      {
        Email = "test@wdid.fyi",
        UserName = "test@wdid.fyi",
        EmailConfirmed = false,
      };
      await userManager.CreateAsync(user);
      var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
      var command = new ConfirmEmailCommand.Command
      {
        UserId = user.Id,
        Token = token,
        ReturnUrl = "https://wdid.fyi",
      };

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.True(response.IsValid);
    });

  [Fact]
  public async Task Should_NotBeValid_When_UserIdIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var user = new DynamoDbUser
      {
        Email = "test@wdid.fyi",
        UserName = "test@wdid.fyi",
        EmailConfirmed = false,
      };
      await userManager.CreateAsync(user);
      var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
      var command = new ConfirmEmailCommand.Command
      {
        Token = token,
        ReturnUrl = "https://wdid.fyi",
      };
      var validator = new ConfirmEmailCommand.CommandValidator(userManager);

      // Act
      var response = await validator.ValidateAsync(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "UserId");
    });

  [Fact]
  public async Task Should_NotBeValid_When_UserDoesntExist() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var user = new DynamoDbUser
      {
        Email = "test@wdid.fyi",
        UserName = "test@wdid.fyi",
        EmailConfirmed = false,
      };
      await userManager.CreateAsync(user);
      var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
      var command = new ConfirmEmailCommand.Command
      {
        UserId = Guid.NewGuid().ToString(),
        Token = token,
        ReturnUrl = "https://wdid.fyi",
      };
      var validator = new ConfirmEmailCommand.CommandValidator(userManager);

      // Act
      var response = await validator.ValidateAsync(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "UserIdInvalid" && error.PropertyName == "UserId");
    });

  [Fact]
  public async Task Should_NotBeValid_When_TokenIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var user = new DynamoDbUser
      {
        Email = "test@wdid.fyi",
        UserName = "test@wdid.fyi",
      };
      await userManager.CreateAsync(user);
      var command = new ConfirmEmailCommand.Command
      {
        UserId = user.Id,
        ReturnUrl = "https://wdid.fyi",
      };
      var validator = new ConfirmEmailCommand.CommandValidator(userManager);

      // Act
      var response = await validator.ValidateAsync(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "Token");
    });

  [Fact]
  public async Task Should_NotBeValid_When_ReturnUrlIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var user = new DynamoDbUser
      {
        Email = "test@wdid.fyi",
        UserName = "test@wdid.fyi",
      };
      await userManager.CreateAsync(user);
      var command = new ConfirmEmailCommand.Command
      {
        UserId = user.Id,
        Token = "a-confirm-token",
      };
      var validator = new ConfirmEmailCommand.CommandValidator(userManager);

      // Act
      var response = await validator.ValidateAsync(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "ReturnUrl");
    });

  [Fact]
  public async Task Should_NotBeValid_When_WithInvalidToken() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var user = new DynamoDbUser
      {
        Email = "test@wdid.fyi",
        UserName = "test@wdid.fyi",
        EmailConfirmed = false,
      };
      await userManager.CreateAsync(user);
      var command = new ConfirmEmailCommand.Command
      {
        UserId = user.Id,
        Token = Guid.NewGuid().ToString(),
        ReturnUrl = "https://wdid.fyi",
      };

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "InvalidToken");
    });
}
