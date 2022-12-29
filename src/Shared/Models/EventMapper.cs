using System.Globalization;

namespace App.Api.Shared.Models;

public static class EventMapper
{
    public static EventDto ToDto(Event instance) => new()
    {
        Id = instance.Id,
        Date = instance.Date,
        Title = instance.Title,
        Description = instance.Description,
    };

    public static Event FromDto(EventDto instance) => new()
    {
        PartitionKey = instance.Date?.ToString("o", CultureInfo.InvariantCulture),
        SortKey = instance.Id.ToString(),
        Id = instance.Id != default ? instance.Id : Guid.NewGuid(),
        Date = instance.Date,
        Title = instance.Title,
        Description = instance.Description,
    };
}