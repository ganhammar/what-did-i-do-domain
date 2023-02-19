using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Models;

namespace TestBase.Helpers;

public static class AccountHelpers
{
  public static Account CreateAccount(AccountDto accountDto)
  {
    var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
    var item = AccountMapper.FromDto(accountDto);
    var client = new AmazonDynamoDBClient();
    var dbContext = new DynamoDBContext(client);
    dbContext.SaveAsync(item, new()
    {
      OverrideTableName = tableName,
    }, CancellationToken.None).GetAwaiter().GetResult();

    return item;
  }
}
