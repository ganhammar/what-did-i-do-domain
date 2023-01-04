namespace App.Api.Shared.Models;

public class AccountDto
{
  public string? Id { get; set; }
  public string? Name { get; set; }
  public DateTime CreateDate { get; set; } = DateTime.UtcNow;
}
