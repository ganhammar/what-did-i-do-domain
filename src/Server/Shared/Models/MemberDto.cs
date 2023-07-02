namespace App.Api.Shared.Models;

public class MemberDto
{
  public string? Id { get; set; }
  public string? Subject { get; set; }
  public string? AccountId { get; set; }
  public string? Email { get; set; }
  public Role Role { get; set; }
  public DateTime CreateDate { get; set; }
}
