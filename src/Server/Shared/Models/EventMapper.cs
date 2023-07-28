using App.Api.Shared.Extensions;

namespace App.Api.Shared.Models;

public static class EventMapper
{
  public static EventDto ToDto(Event instance) => new()
  {
    Id = $"{instance.PartitionKey}&{instance.SortKey}".To64(),
    AccountId = GetAccountId(instance),
    Date = instance.Date,
    Title = instance.Title,
    Description = instance.Description,
    Tags = instance.Tags,
  };

  public static Event FromDto(EventDto instance) => new()
  {
    PartitionKey = instance.Id != default
      ? GetKeys(instance.Id)[0]
      : GetPartitionKey(instance.AccountId!),
    SortKey = instance.Id != default
      ? GetKeys(instance.Id)[1]
      : instance.Date.ToUniversalString(),
    Date = instance.Date,
    Title = instance.Title,
    Description = instance.Description,
    Tags = instance.Tags,
  };

  public static string GetAccountId(Event instance)
    => instance.PartitionKey!.Split("#")[2];

  public static string GetPartitionKey(string accountId)
    => $"EVENT#ACCOUNT#{accountId}";

  public static string[] GetKeys(string? id)
  {
    if (id == default)
    {
      return Array.Empty<string>();
    }

    return id.From64()?.Split('&', 2) ?? Array.Empty<string>();
  }
}
