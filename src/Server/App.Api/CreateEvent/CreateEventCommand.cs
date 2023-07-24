using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using App.Api.Shared.Validators;
using AWS.Lambda.Powertools.Logging;
using FluentValidation;
using MediatR;

namespace App.Api.CreateEvent;

public class CreateEventCommand
{
  public class Command : IRequest<IResponse<EventDto>>
  {
    public string? AccountId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public string[]? Tags { get; set; }
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator()
    {
      RuleFor(x => x.AccountId)
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

    public override async Task<IResponse<EventDto>> Handle(Command request, CancellationToken cancellationToken)
    {
      Logger.LogInformation("Attempting to create Event");

      var item = EventMapper.FromDto(new EventDto
      {
        AccountId = request.AccountId,
        Title = request.Title,
        Description = request.Description,
        Date = request.Date?.ToUniversalTime() ?? DateTime.UtcNow,
        Tags = request.Tags?.Distinct().ToArray(),
      });
      await _client.SaveAsync(item, new()
      {
        OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
      }, cancellationToken);

      Logger.LogInformation("Event created");

      var eventDto = EventMapper.ToDto(item);
      await SaveTags(eventDto, cancellationToken);

      return Response(eventDto);
    }

    public async Task SaveTags(EventDto item, CancellationToken cancellationToken)
    {
      if (item.Tags?.Any() != true)
      {
        return;
      }

      var config = new DynamoDBOperationConfig()
      {
        OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
      };

      Logger.LogInformation($"Attempting to save {item.Tags.Count()} tag(s)");
      var tags = _client.CreateBatchWrite<Tag>(config);
      var eventTags = _client.CreateBatchWrite<EventTag>(config);

      foreach (var value in item.Tags.Distinct())
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

      await tags.ExecuteAsync(cancellationToken);
      await eventTags.ExecuteAsync(cancellationToken);
      Logger.LogInformation("Tags saved");
    }
  }
}
