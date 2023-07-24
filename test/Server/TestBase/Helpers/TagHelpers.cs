using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Models;

namespace TestBase.Helpers;

public static class TagHelpers
{
  public static void CreateTags(string accountId, params string[] tags)
  {
    var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
    var client = new AmazonDynamoDBClient();
    var dbContext = new DynamoDBContext(client);

    var batch = dbContext.CreateBatchWrite<Tag>(new()
    {
      OverrideTableName = tableName,
    });

    foreach (var tag in tags)
    {
      batch.AddPutItem(TagMapper.FromDto(new()
      {
        AccountId = accountId,
        Value = tag,
      }));
    }
    batch.ExecuteAsync().GetAwaiter().GetResult();
  }
}
