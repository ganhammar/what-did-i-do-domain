using App.Api.Shared.Models;

namespace App.Api.ListEvents;

public class Result
{
  public string? PaginationToken { get; set; }
  public List<EventDto> Items { get; set; } = new();
}
