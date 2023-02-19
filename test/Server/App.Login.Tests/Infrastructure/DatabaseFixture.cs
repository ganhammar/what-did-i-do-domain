using Amazon.DynamoDBv2;
using AspNetCore.Identity.AmazonDynamoDB;
using OpenIddict.AmazonDynamoDB;

namespace App.Login.Tests.Infrastructure;

public class DatabaseFixture : IDisposable
{
  public static readonly string IdentityTableName = Guid.NewGuid().ToString();
  public static readonly string OpenIddictTableName = Guid.NewGuid().ToString();
  public static readonly AmazonDynamoDBClient Client = new(new AmazonDynamoDBConfig
  {
    ServiceURL = "http://localhost:8000",
  });
  private bool _disposed;

  public DatabaseFixture()
  {
    CreateTable().GetAwaiter().GetResult();
  }

  private async Task CreateTable()
  {
    await AspNetCoreIdentityDynamoDbSetup.EnsureInitializedAsync(TestUtils.GetIdentityOptions(new()
    {
      Database = Client,
      DefaultTableName = IdentityTableName,
    }));

    await OpenIddictDynamoDbSetup.EnsureInitializedAsync(TestUtils.GetOpenIddictOptions(new()
    {
      Database = Client,
      DefaultTableName = OpenIddictTableName,
    }));
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    Client.DeleteTableAsync(OpenIddictTableName).GetAwaiter().GetResult();
    Client.DeleteTableAsync(IdentityTableName).GetAwaiter().GetResult();
    Client.Dispose();
    _disposed = true;
  }
}
