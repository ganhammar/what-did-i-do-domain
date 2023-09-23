using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using App.Api.Shared.Extensions;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using App.Api.Shared.Validators;
using AWS.Lambda.Powertools.Logging;
using FluentValidation;
using MediatR;

namespace App.Api.ListEvents;

public class ListEventsQuery
{
  public class Query : IRequest<IResponse<Result>>
  {
    public string? AccountId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Limit { get; set; }
    public string? Tag { get; set; }
    public string? PaginationToken { get; set; }
  }

  public class QueryValidator : AbstractValidator<Query>
  {
    public QueryValidator()
    {
      RuleFor(x => x.AccountId)
        .NotEmpty();

      RuleFor(x => x.Limit)
        .NotEmpty()
        .GreaterThan(0)
        .LessThanOrEqualTo(200);

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

      RuleFor(x => x)
        .HasRequiredScopes("event");
    }
  }

  public class QueryHandler : Handler<Query, IResponse<Result>>
  {
    private readonly DynamoDBContext _client;
    private readonly IAmazonDynamoDB _database;
    private JsonSerializerOptions _serializerOptions = new()
    {
      Converters =
      {
        new AttributeValueJsonConverter(),
      },
    };

    public QueryHandler(IAmazonDynamoDB database)
    {
      _client = new DynamoDBContext(database);
      _database = database;
    }

    public override async Task<IResponse<Result>> Handle(Query request, CancellationToken cancellationToken)
    {
      var fromDate = request.FromDate.HasValue
        ? request.FromDate.Value : DateTime.UtcNow.Date;
      var toDate = request.ToDate.HasValue
        ? request.ToDate.Value : DateTime.UtcNow.AddDays(1).Date;

      if (request.Tag != default)
      {
        return await FilterByTag(request, fromDate, toDate, cancellationToken);
      }

      Logger.LogInformation($"Listing Events between {fromDate.ToString("o")} and {toDate.ToString("o")} for account {request.AccountId}");

      var query = await _database.QueryAsync(new()
      {
        KeyConditionExpression = "PartitionKey = :partitionKey AND SortKey BETWEEN :fromDate AND :toDate",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
          { ":partitionKey", new(EventMapper.GetPartitionKey(request.AccountId!)) },
          { ":fromDate", new(fromDate.ToUniversalString()) },
          { ":toDate", new(toDate.ToUniversalString()) },
        },
        Limit = request.Limit,
        ScanIndexForward = false,
        TableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
        ExclusiveStartKey = FromBase64(request.PaginationToken),
      }, cancellationToken);

      var events = query.Items.Select(x =>
      {
        var document = Document.FromAttributeMap(x);
        return _client.FromDocument<Event>(document);
      });

      Logger.LogInformation($"Found {events.Count()} Event(s)");

      return Response(new Result()
      {
        PaginationToken = ToBase64(query.LastEvaluatedKey),
        Items = events.Select(x => EventMapper.ToDto(x)).ToList(),
      });
    }

    private async Task<IResponse<Result>> FilterByTag(
      Query request, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
    {
      Logger.LogInformation($"Listing Events with the tag {request.Tag} between {fromDate.ToString("o")} and {toDate.ToString("o")} for account {request.AccountId}");

      var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
      var query = await _database.QueryAsync(new()
      {
        KeyConditionExpression = "PartitionKey = :partitionKey AND SortKey BETWEEN :fromDate AND :toDate",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
          { ":partitionKey", new(EventTagMapper.GetPartitionKey(request.AccountId)) },
          { ":fromDate", new(EventTagMapper.GetSortKey(request.Tag, fromDate)) },
          { ":toDate", new(EventTagMapper.GetSortKey(request.Tag, toDate)) },
        },
        Limit = request.Limit,
        ScanIndexForward = false,
        TableName = tableName,
        ExclusiveStartKey = FromBase64(request.PaginationToken),
      }, cancellationToken);

      Logger.LogInformation($"Found {query.Items.Count()} Event(s) with matching tag");

      var eventTags = query.Items.Select(x =>
      {
        var document = Document.FromAttributeMap(x);
        return EventTagMapper.ToDto(_client.FromDocument<EventTag>(document));
      });

      var batch = _client.CreateBatchGet<Event>(new()
      {
        OverrideTableName = tableName,
      });

      foreach (var eventTag in eventTags)
      {
        var item = EventMapper.FromDto(new()
        {
          AccountId = eventTag.AccountId,
          Date = eventTag.Date,
        });
        batch.AddKey(item.PartitionKey, item.SortKey);
      }

      await batch.ExecuteAsync(cancellationToken);

      return Response(new Result()
      {
        Items = batch.Results.Select(x => EventMapper.ToDto(x)).ToList(),
        PaginationToken = ToBase64(query.LastEvaluatedKey),
      });
    }

    private string? ToBase64(Dictionary<string, AttributeValue> dictionary)
    {
      if (dictionary.Any() == false)
      {
        return default;
      }

      var json = JsonSerializer.Serialize(dictionary, _serializerOptions);
      return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    private Dictionary<string, AttributeValue>? FromBase64(string? token)
    {
      if (token == default)
      {
        return default;
      }

      var bytes = Convert.FromBase64String(token);
      return JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(
        Encoding.UTF8.GetString(bytes), _serializerOptions);
    }
  }
}
