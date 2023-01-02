using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Models;

namespace TestBase.Helpers;

public static class EventHelpers
{
  public static Event CreateEvent(EventDto eventDto)
  {
    var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
    var item = EventMapper.FromDto(eventDto);
    var client = new AmazonDynamoDBClient();
    var dbContext = new DynamoDBContext(client);
    dbContext.SaveAsync(item, new()
    {
      OverrideTableName = tableName,
    }, CancellationToken.None).GetAwaiter().GetResult();

    return item;
  }
}
