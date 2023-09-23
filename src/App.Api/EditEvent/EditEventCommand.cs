using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using App.Api.Shared.Validators;
using AWS.Lambda.Powertools.Logging;
using FluentValidation;
using MediatR;

namespace App.Api.EditEvent;

public class EditEventCommand
{
  public class Command : IRequest<IResponse<EventDto>>
  {
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator()
    {
      RuleFor(x => x.Id)
        .NotEmpty();

      RuleFor(x => x.Title)
        .NotEmpty();

      RuleFor(x => x)
        .HasRequiredScopes("event");
    }
  }

  public class CommandHandler : Handler<Command, IResponse<EventDto>>
  {
    private readonly DynamoDBContext _client;

    public CommandHandler(IAmazonDynamoDB database)
    {
      _client = new DynamoDBContext(database);
    }

    public override async Task<IResponse<EventDto>> Handle(
      Command request, CancellationToken cancellationToken)
    {
      Logger.LogInformation("Attempting to edit Event");

      var config = new DynamoDBOperationConfig()
      {
        OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
      };
      var item = EventMapper.FromDto(new EventDto
      {
        Id = request.Id,
      });
      item = await _client.LoadAsync<Event>(
        item.PartitionKey, item.SortKey, config, cancellationToken);

      var eventDto = EventMapper.ToDto(item);
      await SaveTags(eventDto, request.Tags, cancellationToken);

      item.Title = request.Title;
      item.Description = request.Description;
      item.Tags = request.Tags;

      await _client.SaveAsync(item, config, cancellationToken);

      Logger.LogInformation("Event editd");

      eventDto = EventMapper.ToDto(item);

      return Response(eventDto);
    }

    public async Task SaveTags(
      EventDto item, string[]? newTags, CancellationToken cancellationToken)
    {
      if (item.Tags?.Any() != true && newTags?.Any() != true)
      {
        return;
      }

      var config = new DynamoDBOperationConfig()
      {
        OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
      };

      Logger.LogInformation($"Attempting to update tag(s)");

      var tags = _client.CreateBatchWrite<Tag>(config);
      var eventTags = _client.CreateBatchWrite<EventTag>(config);

      // Delete old tags that no longer applies
      if (item.Tags?.Any() == true)
      {
        foreach (var value in item.Tags)
        {
          if (newTags?.Any() == false || newTags!.Contains(value) == false)
          {
            eventTags.AddDeleteItem(EventTagMapper.FromDto(new EventTagDto
            {
              AccountId = item.AccountId,
              Date = item.Date,
              Value = value,
            }));
          }
        }
      }

      // Create or update
      if (newTags?.Any() == true)
      {
        foreach (var value in newTags.Distinct())
        {
          tags.AddPutItem(TagMapper.FromDto(new TagDto
          {
            AccountId = item.AccountId,
            Value = value,
          }));

          eventTags.AddPutItem(EventTagMapper.FromDto(new EventTagDto
          {
            AccountId = item.AccountId,
            Date = item.Date,
            Value = value,
          }));
        }
      }

      await tags.ExecuteAsync(cancellationToken);
      await eventTags.ExecuteAsync(cancellationToken);
      Logger.LogInformation("Tags saved");
    }
  }
}
