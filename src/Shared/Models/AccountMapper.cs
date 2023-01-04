using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using App.Api.Shared.Extensions;

namespace App.Api.Shared.Models;

public static class AccountMapper
{
  public static AccountDto ToDto(Account instance) => new()
  {
    Id = GetId(instance.PartitionKey!),
    CreateDate = instance.CreateDate,
    Name = instance.Name,
  };

  public static Account FromDto(AccountDto instance) => new()
  {
    PartitionKey = GetAccountId(instance.Id!),
    SortKey = instance.CreateDate.ToString("o", CultureInfo.InvariantCulture),
    CreateDate = instance.CreateDate,
    Name = instance.Name,
  };

  public static string GetId(string partitionKey)
    => partitionKey.Split('#')[1];

  public static string GetAccountId(string id)
    => $"ACCOUNT#{id}";

  public static async Task<string> GetUniqueId(string name, DynamoDBContext context, CancellationToken cancellationToken)
  {
    var baseKey = GetAccountId(name.UrlFriendly());
    var suffix = 0;
    var key = baseKey;
    var exits = true;

    while (exits)
    {
      var search = context.FromQueryAsync<Account>(
        new()
        {
          KeyExpression = new Expression
          {
            ExpressionStatement = "PartitionKey = :partitionKey",
            ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
            {
              { ":partitionKey", key },
            }
          },
        },
        new()
        {
          OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
        });
      var found = await search.GetNextSetAsync(cancellationToken);

      if (found.Any())
      {
        suffix += 1;
        key = $"{baseKey}-{suffix}";
      }
      else
      {
        exits = false;
      }
    }

    return key;
  }
}
