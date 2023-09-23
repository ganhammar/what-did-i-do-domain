using App.Api.Shared.Extensions;

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
    PartitionKey = GetPartitionKey(instance.AccountId),
    SortKey = GetSortKey(instance.Value, instance.Date),
  };

  public static string GetAccountId(EventTag instance)
    => instance.PartitionKey!.Split("#")[2];

  public static string GetValue(EventTag instance)
    => instance.SortKey!.Split("#")[2];

  public static DateTime? GetDate(EventTag instance)
  {
    var rawDate = instance.SortKey!.Split("#")[4];
    DateTime.TryParse(rawDate, out var date);

    return date;
  }

  public static string GetPartitionKey(string? accountId)
    => $"EVENT_TAG#ACCOUNT#{accountId}";

  public static string GetSortKey(string? value, DateTime? date)
    => $"#TAG#{value}#DATE#{date.ToUniversalString()}";
}
