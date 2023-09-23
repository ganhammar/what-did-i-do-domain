using System.Text.RegularExpressions;
using App.Api.Shared.Extensions;

namespace App.Api.Shared.Models;

public static class MemberMapper
{
  public static MemberDto ToDto(Member instance) => new()
  {
    Id = $"{instance.PartitionKey}&{instance.SortKey}".To64(),
    AccountId = GetAccountId(instance),
    Subject = instance.Subject,
    Role = GetRole(instance),
    CreateDate = instance.CreateDate,
    Email = instance.Email,
  };

  public static Member FromDto(MemberDto instance) => new()
  {
    PartitionKey = instance.Id != default
      ? GetKeys(instance.Id)[0]
      : GetPartitionKey(instance.AccountId!),
    SortKey = instance.Id != default
      ? GetKeys(instance.Id)[1]
      : GetSortKey(instance),
    CreateDate = instance.CreateDate,
    Subject = instance.Subject,
    Email = instance.Email,
  };

  public static string GetAccountId(Member instance)
    => instance.PartitionKey!.Split("#")[1];

  public static string GetPartitionKey(string accountId)
    => $"MEMBER#{accountId}";

  public static string GetSortKey(MemberDto instance)
    => $"#ROLE#{instance.Role.ToString()}#USER#{instance.Subject}";

  public static Role GetRole(Member instance)
    => Enum.Parse<Role>(Regex.Match(instance.SortKey!, "#ROLE#(.+?)#USER#").Groups[1].Value);

  public static string[] GetKeys(string? id)
  {
    if (id == default)
    {
      return Array.Empty<string>();
    }

    return id.From64()?.Split('&', 2) ?? Array.Empty<string>();
  }
}
