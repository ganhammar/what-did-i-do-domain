using App.Login.Features.Email;
using App.Login.Features.User;
using App.Login.Infrastructure.Validators;
using App.Login.Tests.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace App.Login.Tests.Features.User;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class SendCodeCommandTests : TestBase
{
  [Fact]
  public async Task Should_SendEmail_When_CommandIsValid() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var email = "valid@gomsle.com";
      var password = "itsaseasyas123";
      var user = new DynamoDbUser
      {
        Email = email,
        UserName = email,
        EmailConfirmed = true,
        TwoFactorEnabled = true,
      };
      await userManager.CreateAsync(user, password);
      await mediator.Send(new LoginCommand.Command
      {
        Email = email,
        Password = password,
        RememberMe = false,
      });
      var command = new SendCodeCommand.Command
      {
        Provider = "Email",
      };

      // Act
      var result = await mediator.Send(command);

      // Assert
      Assert.True(result.IsValid);

      var mock = GetMock<IEmailSender>();
      mock!.Verify(x =>
        x.Send(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
        Times.Once());
    });

  [Fact]
  public async Task Should_NotBeValid_When_ProviderIsNotSet() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var email = "valid@gomsle.com";
      var password = "itsaseasyas123";
      var user = new DynamoDbUser
      {
        Email = email,
        UserName = email,
        EmailConfirmed = true,
        TwoFactorEnabled = true,
      };
      await userManager.CreateAsync(user, password);
      await mediator.Send(new LoginCommand.Command
      {
        Email = email,
        Password = password,
        RememberMe = false,
      });
      var command = new SendCodeCommand.Command();

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "Provider");
    });

  [Fact]
  public async Task Should_NotBeValid_When_NoLoginAttempIsInProgress() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var command = new SendCodeCommand.Command
      {
        Provider = "Email",
      };

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == nameof(ErrorCodes.NoLoginAttemptInProgress));
    });

  [Fact]
  public async Task Should_NotBeValid_When_ProviderIsAllowed() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var email = "valid@gomsle.com";
      var password = "itsaseasyas123";
      var user = new DynamoDbUser
      {
        Email = email,
        UserName = email,
        EmailConfirmed = true,
        TwoFactorEnabled = true,
      };
      await userManager.CreateAsync(user, password);
      await mediator.Send(new LoginCommand.Command
      {
        Email = email,
        Password = password,
        RememberMe = false,
      });
      var command = new SendCodeCommand.Command
      {
        Provider = "Phone",
      };

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.False(response.IsValid);
      Assert.Contains(response.Errors, error =>
        error.ErrorCode == nameof(ErrorCodes.TwoFactorProviderNotValid) && error.PropertyName == "Provider");
    });
}
