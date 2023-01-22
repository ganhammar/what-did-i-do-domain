using App.Login.Features.User;
using App.Login.Tests.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace App.Login.Tests.Features.User;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class ResetPasswordCommandTests : TestBase
{
  [Fact]
  public async Task Should_ResetPasswordCommand_When_CommandIsValid() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var user = new DynamoDbUser
      {
        Email = "test@gomsle.com",
        UserName = "test",
      };
      await userManager.CreateAsync(user);
      var token = await userManager.GeneratePasswordResetTokenAsync(user);
      var command = new ResetPasswordCommand.Command
      {
        UserId = user.Id,
        Token = token,
        Password = "itsnotaseasyas123",
        ReturnUrl = "https://gomsle.com/login",
      };

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.True(response.IsValid);
    });

  [Fact]
  public async Task Should_NotResetPasswordCommand_When_TokenIsInvalid() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var user = new DynamoDbUser
      {
        Email = "test@gomsle.com",
        UserName = "test",
      };
      await userManager.CreateAsync(user);
      var command = new ResetPasswordCommand.Command
      {
        UserId = user.Id,
        Token = "not-the-right-stuff",
        Password = "itsnotaseasyas123",
        ReturnUrl = "https://gomsle.com/login",
      };

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error => error.ErrorCode == "InvalidToken");
    });

  [Fact]
  public async Task Should_NotBeValid_When_UserIdIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var command = new ResetPasswordCommand.Command
      {
        Token = "a-reset-token",
        Password = "itsnotaseasyas123",
        ReturnUrl = "https://gomsle.com/login",
      };
      var validator = new ResetPasswordCommand.CommandValidator();

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "UserId");
    });

  [Fact]
  public async Task Should_NotBeValid_When_TokenIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var command = new ResetPasswordCommand.Command
      {
        UserId = "a-user-id",
        Password = "itsnotaseasyas123",
        ReturnUrl = "https://gomsle.com/login",
      };
      var validator = new ResetPasswordCommand.CommandValidator();

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "Token");
    });

  [Fact]
  public async Task Should_NotBeValid_When_PasswordIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var command = new ResetPasswordCommand.Command
      {
        UserId = "a-user-id",
        Token = "a-reset-token",
        ReturnUrl = "https://gomsle.com/login",
      };
      var validator = new ResetPasswordCommand.CommandValidator();

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "Password");
    });

  [Fact]
  public async Task Should_NotBeValid_When_ReturnUrlIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var command = new ResetPasswordCommand.Command
      {
        UserId = "a-user-id",
        Token = "a-reset-token",
        Password = "itsnotaseasyas123",
      };
      var validator = new ResetPasswordCommand.CommandValidator();

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "ReturnUrl");
    });
}
