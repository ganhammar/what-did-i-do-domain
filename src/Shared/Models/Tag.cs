namespace App.Api.Shared.Models;

public class Tag
{
  public string? PartitionKey { get; set; } // TAG#ACCOUNT#{ACCOUNT_ID}
  public string? SortKey { get; set; } // VALUE
}
