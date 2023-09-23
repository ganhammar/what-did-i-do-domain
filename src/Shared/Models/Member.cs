namespace App.Api.Shared.Models;

public class Member
{
  public string? PartitionKey { get; set; }
  public string? SortKey { get; set; }
  public string? Subject { get; set; }
  public string? Email { get; set; }
  public DateTime CreateDate { get; set; }
}
