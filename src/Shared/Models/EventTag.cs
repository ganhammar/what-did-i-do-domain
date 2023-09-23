namespace App.Api.Shared.Models;

public class EventTag
{
  public string? PartitionKey { get; set; } // EVENT_TAG#ACCOUNT#{ACCOUNT_ID}
  public string? SortKey { get; set; } // #TAG#{TAG}#DATE#{DATE}
}
