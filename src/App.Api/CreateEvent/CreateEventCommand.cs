using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using FluentValidation;
using MediatR;

namespace App.Api.CreateEvent;

public class CreateEventCommand
{
  public class Command : IRequest<IResponse<EventDto>>
  {
    public Guid AccountId { get; set; }
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
      var item = EventMapper.FromDto(new EventDto
      {
        Title = request.Title,
        Description = request.Description,
        Date = request.Date?.ToUniversalTime() ?? DateTime.UtcNow,
        Tags = request.Tags,
      });
      await _client.SaveAsync(item, new()
      {
        OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
      }, cancellationToken);

      return Response(EventMapper.ToDto(item));
    }
  }
}
