namespace App.Api.Shared.Models;

public class Event
{
    public string? PartitionKey { get; set; }
    public string? SortKey { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
    public DateTime? Date { get; set; }
}