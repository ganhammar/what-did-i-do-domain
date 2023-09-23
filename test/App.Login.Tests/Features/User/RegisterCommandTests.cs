﻿using App.Login.Features.User;
using App.Login.Tests.Infrastructure;

namespace App.Login.Tests.Features.User;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class RegisterCommandTests : TestBase
{
  [Fact]
  public async Task Should_RegisterUser_When_CommandIsValid() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var email = "test@wdid.fyi";
      var command = new RegisterCommand.Command
      {
        Email = email,
        UserName = "test",
        Password = "itsaseasyas123",
        ReturnUrl = "https://wdid.fyi",
      };

      // Act
      var response = await mediator.Send(command);

      // Assert
      Assert.True(response.IsValid);
      Assert.Equal(email, response.Result!.Email);
    });
}
