using App.Login.Features.Email;
using App.Login.Features.User;
using App.Login.Infrastructure.Validators;
using App.Login.Tests.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace App.Login.Tests.Features.User;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class UserControllerTests : TestBase
{
  private Func<IServiceProvider, object[]> ConfigureController = (services) =>
  {
    var mediator = services.GetRequiredService<IMediator>();

    return new object[] { mediator };
  };

  [Fact]
  public async Task Should_RegisterUser_When_RequestIsValid() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      var email = "test@wdid.fyi";
      var result = await controller.Register(new()
      {
        Email = email,
        Password = "itsaseasyas123",
        ReturnUrl = "http://wdid.fyi",
      });

      Assert.NotNull(result);

      var okResult = result as OkObjectResult;

      Assert.NotNull(okResult);

      var response = okResult!.Value as DynamoDbUser;

      Assert.NotNull(response);
      Assert.Equal(email, response!.Email);

      var mock = GetMock<IEmailSender>();
      mock!.Verify(x =>
        x.Send(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
        Times.Once());
    });

  [Fact]
  public async Task Should_SendEmail_When_Registering() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      var email = "test@wdid.fyi";
      var result = await controller.Register(new()
      {
        Email = email,
        Password = "itsaseasyas123",
        ReturnUrl = "http://wdid.fyi",
      }) as OkObjectResult;

      var response = result!.Value as DynamoDbUser;

      var mock = GetMock<IEmailSender>();
      mock!.Verify(x =>
        x.Send(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
        Times.Once());
    });

  [Fact]
  public async Task Should_ReturnBadRequest_When_UserAlreadyExists() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      var email = "test@wdid.fyi";

      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      await userManager.CreateAsync(new()
      {
        Email = email,
        UserName = email,
      });

      var result = await controller.Register(new()
      {
        Email = email,
        Password = "itsaseasyas123",
        ReturnUrl = "http://wdid.fyi",
      });

      Assert.NotNull(result);

      var badRequestResult = result as BadRequestObjectResult;

      Assert.NotNull(badRequestResult);

      var errors = badRequestResult!.Value as IEnumerable<ValidationFailure>;

      Assert.NotNull(errors);
      Assert.Contains(errors, error => error.ErrorCode == "DuplicateEmail");
    });

  [Fact]
  public async Task Should_ReturnBadRequest_When_EmailIsNotSet() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      var result = await controller.Register(new()
      {
        Email = string.Empty,
        Password = "itsaseasyas123",
        ReturnUrl = "http://wdid.fyi",
      });

      Assert.NotNull(result);

      var badRequestResult = result as BadRequestObjectResult;

      Assert.NotNull(badRequestResult);

      var errors = badRequestResult!.Value as IEnumerable<ValidationFailure>;

      Assert.NotNull(errors);
      Assert.Contains(errors, error =>
        error.ErrorCode == "NotEmptyValidator" && error.PropertyName == "Email");
    });

  [Fact]
  public async Task Should_ConfirmEmailCommand_When_RequestIsValid() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
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
      var url = "https://wdid.fyi/my-client-app";

      // Act
      var result = await controller.ConfirmEmail(new()
      {
        ReturnUrl = url,
        Token = token,
        UserId = user.Id,
      });

      // Assert
      Assert.NotNull(result);

      var redirectResult = result as RedirectResult;

      Assert.NotNull(redirectResult);
      Assert.Equal(url, redirectResult!.Url);
    });

  [Fact]
  public async Task Should_ReturnForbidden_When_ConfirmRequestIsInvalid() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Act
      var result = await controller.ConfirmEmail(new());

      // Assert
      Assert.NotNull(result);

      var forbidResult = result as ForbidResult;

      Assert.NotNull(forbidResult);
    });

  [Fact]
  public async Task Should_SendResetEmail_When_ForgotRequestIsValid() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var email = "test@wdid.fyi";
      var user = new DynamoDbUser
      {
        Email = email,
        UserName = "test@wdid.fyi",
        EmailConfirmed = false,
      };
      await userManager.CreateAsync(user);
      var command = new ForgotPasswordCommand.Command
      {
        Email = email,
        ResetUrl = "https://wdid.fyi/reset",
      };

      // Act
      var result = await controller.Forgot(command);

      // Assert
      Assert.NotNull(result);
      Assert.IsType<NoContentResult>(result);

      var mock = GetMock<IEmailSender>();
      mock!.Verify(x =>
        x.Send(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
        Times.Once());
    });

  [Fact]
  public async Task Should_ResetPasswordCommand_When_RequestIsValid() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
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
      var token = await userManager.GeneratePasswordResetTokenAsync(user);
      var url = "https://wdid.fyi/my-client-app";

      // Act
      var result = await controller.Reset(new()
      {
        ReturnUrl = url,
        Token = token,
        UserId = user.Id,
        Password = "itsaseasyas123",
      });

      // Assert
      Assert.NotNull(result);

      var redirectResult = result as RedirectResult;

      Assert.NotNull(redirectResult);
      Assert.Equal(url, redirectResult!.Url);
    });

  [Fact]
  public async Task Should_ReturnForbidden_When_ResetRequestIsInvalid() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Act
      var result = await controller.Reset(new());

      // Assert
      Assert.NotNull(result);

      var forbidResult = result as ForbidResult;

      Assert.NotNull(forbidResult);
    });

  [Fact]
  public async Task Should_LoginUser_When_RequestIsValid() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var email = "valid@wdid.fyi";
      var password = "itsaseasyas123";
      await userManager.CreateAsync(new()
      {
        Email = email,
        UserName = email,
        EmailConfirmed = true,
      });

      // Act
      var result = await controller.Login(new()
      {
        Email = email,
        Password = password,
      });

      // Assert
      Assert.NotNull(result);

      var okObjectResult = result as OkObjectResult;
      Assert.NotNull(okObjectResult);

      var signInResult = okObjectResult!.Value as Microsoft.AspNetCore.Identity.SignInResult;
      Assert.NotNull(signInResult);
      Assert.True(signInResult!.Succeeded);
    });

  [Fact]
  public async Task Should_ReturnListOfTwoFactorProviders_When_LoginIsInProgress() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var email = "valid@wdid.fyi";
      var password = "itsaseasyas123";
      await userManager.CreateAsync(new()
      {
        Email = email,
        UserName = email,
        EmailConfirmed = true,
        TwoFactorEnabled = true,
      });
      await controller.Login(new()
      {
        Email = email,
        Password = password,
      });

      // Act
      var result = await controller.GetTwoFactorProviders(new());

      // Assert
      Assert.NotNull(result);

      var okObjectResult = result as OkObjectResult;
      Assert.NotNull(okObjectResult);

      var providers = okObjectResult!.Value as List<string>;
      Assert.NotNull(providers);
      Assert.Contains(providers, x => x == "Email");
    });

  [Fact]
  public async Task Should_ReturnBadRequest_When_GettingListOfTwoFactorProvidersAndNoLoginIsInProgress() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Act
      var result = await controller.GetTwoFactorProviders(new());

      // Assert
      Assert.NotNull(result);

      var badRequestObjectResult = result as BadRequestObjectResult;
      Assert.NotNull(badRequestObjectResult);
    });

  [Fact]
  public async Task Should_RetunNoContent_When_SendingCodeAndLoginIsInProgress() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Arrange
      var userManager = services.GetRequiredService<UserManager<DynamoDbUser>>();
      var email = "valid@wdid.fyi";
      var password = "itsaseasyas123";
      await userManager.CreateAsync(new()
      {
        Email = email,
        UserName = email,
        EmailConfirmed = true,
        TwoFactorEnabled = true,
      });
      await controller.Login(new()
      {
        Email = email,
        Password = password,
      });

      // Act
      var result = await controller.SendCodeCommand(new()
      {
        Provider = "Email",
      });

      // Assert
      Assert.NotNull(result);

      var noContentResult = result as NoContentResult;
      Assert.NotNull(noContentResult);
    });

  [Fact]
  public async Task Should_ReturnBadRequest_When_SendingCodeAndNoLoginIsInProgress() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Act
      var result = await controller.SendCodeCommand(new()
      {
        Provider = "Email",
      });

      // Assert
      Assert.NotNull(result);

      var badRequestObjectResult = result as BadRequestObjectResult;
      Assert.NotNull(badRequestObjectResult);
    });

  [Fact]
  public async Task Should_BeSuccessfull_When_VerifyingCodeThatIsCorrect() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
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
        TwoFactorEnabled = true,
      };
      await userManager.CreateAsync(user);
      await controller.Login(new()
      {
        Email = email,
        Password = password,
      });
      var code = await userManager.GenerateTwoFactorTokenAsync(user, "Email");

      // Act
      var result = await controller.VerifyCodeCommand(new()
      {
        Provider = "Email",
        Code = code,
      });

      // Assert
      Assert.NotNull(result);

      var okObjectResult = result as OkObjectResult;
      Assert.NotNull(okObjectResult);

      var signInResult = okObjectResult!.Value as Microsoft.AspNetCore.Identity.SignInResult;
      Assert.NotNull(signInResult);
      Assert.True(signInResult!.Succeeded);
    });

  [Fact]
  public async Task Should_BeSuccessfull_When_GettingLoggedInUser() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Arrange
      var user = await CreateAndLoginValidUser(services);

      // Act
      var result = await controller.GetCurrentUser(new());

      // Assert
      Assert.NotNull(result);

      var okObjectResult = result as OkObjectResult;
      Assert.NotNull(okObjectResult);

      var currentUser = okObjectResult!.Value as DynamoDbUser;
      Assert.NotNull(currentUser);
      Assert.Equal(user.Id, currentUser.Id);
    });

  [Fact]
  public async Task Should_ReturnBadRequest_When_GettingCurrentUserAndNotLoggedIn() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Act
      var result = await controller.GetCurrentUser(new());

      // Assert
      Assert.NotNull(result);

      var badRequestObjectResult = result as BadRequestObjectResult;
      Assert.NotNull(badRequestObjectResult);

      var errors = badRequestObjectResult!.Value as IEnumerable<ValidationFailure>;

      Assert.NotNull(errors);
      Assert.Contains(errors, error => error.ErrorCode == nameof(ErrorCodes.NoLoggedInUser));
    });

  [Fact]
  public async Task Should_BeSuccessfull_When_LoggingOutLoggedInUser() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Arrange
      var user = await CreateAndLoginValidUser(services);

      // Act
      var result = await controller.Logout(new());

      // Assert
      Assert.NotNull(result);

      var noContentResult = result as NoContentResult;
      Assert.NotNull(noContentResult);
    });

  [Fact]
  public async Task Should_ReturnBadRequest_When_LoggingOutAndNotLoggedIn() => await ControllerTest<UserController>(
    // Arrange
    ConfigureController,
    // Act & Assert
    async (controller, services) =>
    {
      // Act
      var result = await controller.Logout(new());

      // Assert
      Assert.NotNull(result);

      var badRequestObjectResult = result as BadRequestObjectResult;
      Assert.NotNull(badRequestObjectResult);

      var errors = badRequestObjectResult!.Value as IEnumerable<ValidationFailure>;

      Assert.NotNull(errors);
      Assert.Contains(errors, error => error.ErrorCode == nameof(ErrorCodes.NoLoggedInUser));
    });
}
