using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using App.Api.Shared.Validators;
using AWS.Lambda.Powertools.Logging;
using FluentValidation;
using MediatR;

namespace App.Api.DeleteEvent;

public class DeleteEventCommand
{
  public const string InvalidId = "INVALID_ID_FORMAT";

  public class Command : IRequest<IResponse>
  {
    public string? Id { get; set; }
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator()
    {
      RuleFor(x => x.Id)
        .NotEmpty()
        .Must((command, id) =>
        {
          var keys = EventMapper.GetKeys(id);
          return keys.Length == 2;
        })
        .WithErrorCode(InvalidId)
        .WithMessage("The Id format is invalid");

      RuleFor(x => x)
        .HasRequiredScopes("event");
    }
  }

  public class CommandHandler : Handler<Command, IResponse>
  {
    private readonly DynamoDBContext _client;
    private readonly DynamoDBOperationConfig _config;

    public CommandHandler(IAmazonDynamoDB database)
    {
      _client = new DynamoDBContext(database);
      _config = new()
      {
        OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
      };
    }

    public override async Task<IResponse> Handle(Command request, CancellationToken cancellationToken)
    {
      Logger.LogInformation("Attempting to delete Event");

      var keys = EventMapper.GetKeys(request.Id);
      var item = await _client.LoadAsync<Event>(keys[0], keys[1], _config, cancellationToken);

      if (item != default)
      {
        Logger.LogInformation("Matching Event found, deleting");
        await _client.DeleteAsync(item, _config, cancellationToken);
        await DeleteEventTags(item, cancellationToken);
      }

      return Response();
    }

    public async Task DeleteEventTags(Event item, CancellationToken cancellationToken)
    {
      if (item.Tags?.Any() != true)
      {
        return;
      }

      var eventDto = EventMapper.ToDto(item);
      var batch = _client.CreateBatchWrite<EventTag>(_config);

      foreach (var tag in eventDto.Tags!)
      {
        batch.AddDeleteItem(EventTagMapper.FromDto(new()
        {
          AccountId = eventDto.AccountId,
          Date = eventDto.Date,
          Value = tag,
        }));
      }

      await batch.ExecuteAsync(cancellationToken);
    }
  }
}
