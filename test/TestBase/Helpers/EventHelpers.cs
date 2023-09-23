﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Models;

namespace TestBase.Helpers;

public static class EventHelpers
{
  public static Event CreateEvent(EventDto eventDto)
  {
    var config = new DynamoDBOperationConfig()
    {
      OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
    };
    var item = EventMapper.FromDto(eventDto);
    var client = new AmazonDynamoDBClient();
    var dbContext = new DynamoDBContext(client);
    dbContext.SaveAsync(item, config, CancellationToken.None).GetAwaiter().GetResult();

    if (eventDto.Tags?.Any() == true)
    {
      var batch = dbContext.CreateBatchWrite<EventTag>(config);

      foreach (var value in eventDto.Tags)
      {
        batch.AddPutItem(EventTagMapper.FromDto(new()
        {
          AccountId = eventDto.AccountId,
          Date = eventDto.Date,
          Value = value,
        }));
      }

      batch.ExecuteAsync().GetAwaiter().GetResult();
    }

    return item;
  }
}
