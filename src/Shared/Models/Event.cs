namespace App.Api.Shared.Models;

public class Event
{
    public Event(string title)
    {
        Title = title;
    }

    public string Title { get; set; }
}