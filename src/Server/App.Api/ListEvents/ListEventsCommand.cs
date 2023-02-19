using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using AWS.Lambda.Powertools.Logging;
using FluentValidation;
using MediatR;

namespace App.Api.ListEvents;

public class ListEventsCommand
{
  public class Command : IRequest<IResponse<List<EventDto>>>
  {
    public string? AccountId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator()
    {
      RuleFor(x => x.AccountId)
        .NotEmpty();

      When(x => x.ToDate.HasValue, () =>
      {
        RuleFor(x => x.FromDate)
          .NotEmpty()
          .LessThan(x => x.ToDate);
      });

      When(x => x.FromDate.HasValue, () =>
      {
        RuleFor(x => x.ToDate)
          .NotEmpty();
      });
    }
  }

  public class CommandHandler : Handler<Command, IResponse<List<EventDto>>>
  {
    private readonly DynamoDBContext _client;

    public CommandHandler(IAmazonDynamoDB database)
    {
      _client = new DynamoDBContext(database);
    }

    public override async Task<IResponse<List<EventDto>>> Handle(Command request, CancellationToken cancellationToken)
    {
      var fromDate = request.FromDate.HasValue
        ? request.FromDate.Value : DateTime.UtcNow.Date;
      var toDate = request.ToDate.HasValue
        ? request.ToDate.Value : DateTime.UtcNow.AddDays(1).Date;

      Logger.LogInformation($"Listing Events between {fromDate.ToString("o")} and {toDate.ToString("o")} for account {request.AccountId}");

      var search = _client.FromQueryAsync<Event>(
        new()
        {
          KeyExpression = new Expression
          {
            ExpressionStatement = "PartitionKey = :partitionKey AND SortKey BETWEEN :fromDate and :toDate",
            ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
            {
              { ":partitionKey", EventMapper.GetPartitionKey(request.AccountId!) },
              { ":fromDate", fromDate },
              { ":toDate", toDate },
            }
          },
        },
        new()
        {
          OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
        });
      var events = await search.GetRemainingAsync(cancellationToken);

      Logger.LogInformation($"Found {events.Count} Event(s)");

      return Response(events.Select(x => EventMapper.ToDto(x)).ToList());
    }
  }
}
