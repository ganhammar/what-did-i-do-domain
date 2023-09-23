namespace App.Api.Shared.Models;

public static class TagMapper
{
  public static TagDto ToDto(Tag instance) => new()
  {
    AccountId = GetAccountId(instance),
    Value = instance.SortKey,
  };

  public static Tag FromDto(TagDto instance) => new()
  {
    PartitionKey = GetPartitionKey(instance.AccountId!),
    SortKey = instance.Value,
  };

  public static string GetAccountId(Tag instance)
    => instance.PartitionKey!.Split("#")[2];

  public static string GetPartitionKey(string accountId)
    => $"TAG#ACCOUNT#{accountId}";
}
