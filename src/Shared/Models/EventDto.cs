namespace App.Api.Shared.Models;

public class EventDto
{
  public string? Id { get; set; }
  public Guid AccountId { get; set; }
  public string? Title { get; set; }
  public string? Description { get; set; }
  public string[]? Tags { get; set; }
  public DateTime? Date { get; set; }
}
