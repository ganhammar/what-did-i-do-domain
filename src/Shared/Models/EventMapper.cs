using System.Globalization;
using App.Api.Shared.Extensions;

namespace App.Api.Shared.Models;

public static class EventMapper
{
  public static EventDto ToDto(Event instance) => new()
  {
    Id = $"{instance.PartitionKey}#{instance.SortKey}".To64(),
    Date = instance.Date,
    Title = instance.Title,
    Description = instance.Description,
  };

  public static Event FromDto(EventDto instance) => new()
  {
    PartitionKey = instance.Id != default
      ? GetKeys(instance.Id)[0]
      : instance.Date?.ToString("o", CultureInfo.InvariantCulture),
    SortKey = instance.Id != default
      ? GetKeys(instance.Id)[1]
      : $"{instance.Title?.UrlFriendly()}-{Guid.NewGuid()}",
    Date = instance.Date,
    Title = instance.Title,
    Description = instance.Description,
  };

  public static string[] GetKeys(string? id)
  {
    if (id == default)
    {
      return Array.Empty<string>();
    }

    return id.From64()?.Split('#') ?? Array.Empty<string>();
  }
}
