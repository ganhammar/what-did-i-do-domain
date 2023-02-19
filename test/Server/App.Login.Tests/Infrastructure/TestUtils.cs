using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.Extensions.Options;
using Moq;
using OpenIddict.AmazonDynamoDB;

namespace App.Login.Tests.Infrastructure;

public static class TestUtils
{
  public static IOptionsMonitor<DynamoDbOptions> GetIdentityOptions(DynamoDbOptions options)
  {
    options.DefaultTableName = options.DefaultTableName == "identity"
      ? DatabaseFixture.IdentityTableName : options.DefaultTableName;
    var mock = new Mock<IOptionsMonitor<DynamoDbOptions>>();
    mock.Setup(x => x.CurrentValue).Returns(options);
    return mock.Object;
  }

  public static IOptionsMonitor<OpenIddictDynamoDbOptions> GetOpenIddictOptions(OpenIddictDynamoDbOptions options)
  {
    options.DefaultTableName = options.DefaultTableName == "openiddict"
      ? DatabaseFixture.OpenIddictTableName : options.DefaultTableName;
    var mock = new Mock<IOptionsMonitor<OpenIddictDynamoDbOptions>>();
    mock.Setup(x => x.CurrentValue).Returns(options);
    return mock.Object;
  }
}
