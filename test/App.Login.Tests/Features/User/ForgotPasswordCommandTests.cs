using App.Login.Features.Email;
using App.Login.Features.User;
using App.Login.Tests.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace App.Login.Tests.Features.User;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class ForgotPasswordCommandTests : TestBase
{
  [Fact]
  public async Task Should_SendResetEmail_When_CommandIsValid() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var email = "test@wdid.fyi";
      var user = new DynamoDbUser
      {
        Email = email,
        UserName = email,
      };
      await userManager.CreateAsync(user);
      var command = new ForgotPasswordCommand.Command
      {
        Email = email,
        ResetUrl = "https://wdid.fyi/reset",
      };

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.True(response.IsValid);

      var mock = GetMock<IEmailSender>();
      mock!.Verify(x =>
        x.Send(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
        Times.Once());
    });

  [Fact]
  public async Task Should_NotSendResetEmail_When_UserDoesntExist() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var email = "test@wdid.fyi";
      var command = new ForgotPasswordCommand.Command
      {
        Email = email,
        ResetUrl = "https://wdid.fyi/reset",
      };

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.True(response.IsValid);

      var mock = GetMock<IEmailSender>();
      mock!.Verify(x =>
        x.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
        Times.Never());
    });

  [Fact]
  public async Task Should_NotBeValid_When_EmailIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var command = new ForgotPasswordCommand.Command
      {
        ResetUrl = "https://wdid.fyi/reset",
      };
      var validator = new ForgotPasswordCommand.CommandValidator();

      // Act
      var response = await validator.ValidateAsync(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "Email");
    });

  [Fact]
  public async Task Should_NotBeValid_When_ResetUrlIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var command = new ForgotPasswordCommand.Command
      {
        Email = "test@wdid.fyi",
      };
      var validator = new ForgotPasswordCommand.CommandValidator();

      // Act
      var response = await validator.ValidateAsync(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "ResetUrl");
    });

  [Fact]
  public async Task Should_NotBeValid_When_EmailIsNotEmailAddress() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var command = new ForgotPasswordCommand.Command
      {
        Email = "not-a-email",
        ResetUrl = "https://wdid.fyi/reset",
      };
      var validator = new ForgotPasswordCommand.CommandValidator();

      // Act
      var response = await validator.ValidateAsync(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "EmailValidator" && error.PropertyName == "Email");
    });
}
