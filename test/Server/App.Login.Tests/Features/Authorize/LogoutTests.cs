using App.Login.Features.Authorization;
using App.Login.Tests.Infrastructure;

namespace App.Login.Tests.Features.Authorize;

[Collection(Constants.DatabaseCollection)]
[Cleanup]
public class LogoutCommandTests : TestBase
{
  [Fact]
  public async Task Should_BeSuccessful_When_LoggingOut() =>
    await MediatorTest(async (mediator, services) =>
    {
      // Arrange
      var command = new LogoutCommand.Command();

      // Act
      var result = await mediator.Send(command);

      // Assert
      Assert.True(result.IsValid);
    });
}
