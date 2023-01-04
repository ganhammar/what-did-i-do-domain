namespace App.Api.Shared.Models;

public class Account
{
  public string? PartitionKey { get; set; }
  public string? SortKey { get; set; }
  public string? Name { get; set; }
  public DateTime CreateDate { get; set; }
}
