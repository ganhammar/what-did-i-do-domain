using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
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

  public static Member AddOwner(Account account, string subject, string email)
  {
    var accountDto = AccountMapper.ToDto(account);
    var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
    var item = MemberMapper.FromDto(new()
    {
      AccountId = accountDto.Id,
      Subject = subject,
      Email = email,
      Role = Role.Owner,
      CreateDate = DateTime.UtcNow,
    });
    var client = new AmazonDynamoDBClient();
    var dbContext = new DynamoDBContext(client);
    dbContext.SaveAsync(item, new()
    {
      OverrideTableName = tableName,
    }, CancellationToken.None).GetAwaiter().GetResult();

    while (IsIndexUpdated(client, subject) == false)
    {
      Thread.Sleep(1000);
    }

    return item;
  }

  private static bool IsIndexUpdated(AmazonDynamoDBClient client, string subject)
  {
    var context = new DynamoDBContext(client);
    var config = new DynamoDBOperationConfig
    {
      OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
    };
    var search = context.FromQueryAsync<Member>(new()
    {
      IndexName = "Subject-index",
      KeyExpression = new Expression
      {
        ExpressionStatement = "Subject = :subject AND begins_with(PartitionKey, :partitionKey)",
        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
        {
          { ":subject", subject },
          { ":partitionKey", "MEMBER#" },
        }
      },
    }, config);

    var result = search.GetRemainingAsync().GetAwaiter().GetResult();

    return result.Any();
  }
}
