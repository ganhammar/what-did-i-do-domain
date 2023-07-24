using System.Globalization;

namespace App.Api.Shared.Models;

public static class EventTagMapper
{
  public static EventTagDto ToDto(EventTag instance) => new()
  {
    AccountId = GetAccountId(instance),
    Value = GetValue(instance),
    Date = GetDate(instance),
  };

  public static EventTag FromDto(EventTagDto instance) => new()
  {
    PartitionKey = GetPartitionKey(instance),
    SortKey = instance.Date?.ToUniversalTime()
      .ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture),
  };

  public static string GetAccountId(EventTag instance)
    => instance.PartitionKey!.Split("#")[1];

  public static string GetValue(EventTag instance)
    => instance.PartitionKey!.Split("#")[3];

  public static DateTime? GetDate(EventTag instance)
  {
    DateTime.TryParse(instance.SortKey, out var date);

    return date;
  }

  public static string GetPartitionKey(EventTagDto instance)
    => $"ACCOUNT#{instance.AccountId}#VALUE#{instance.Value}";
}
