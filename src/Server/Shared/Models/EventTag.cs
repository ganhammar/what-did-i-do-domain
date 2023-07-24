namespace App.Api.Shared.Models;

public class EventTag
{
  public string? PartitionKey { get; set; } // TAG#ACCOUNT#{ACCOUNT_ID}#VALUE#{TAG_VALUE}
  public string? SortKey { get; set; } // {DATE}
}
