using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using App.Api.Shared.Validators;
using AWS.Lambda.Powertools.Logging;
using FluentValidation;
using MediatR;

namespace App.Api.ListTags;

public class ListTagsQuery
{
  public class Query : IRequest<IResponse<List<TagDto>>>
  {
    public string? AccountId { get; set; }
  }

  public class QueryValidator : AbstractValidator<Query>
  {
    public QueryValidator()
    {
      RuleFor(x => x.AccountId)
        .NotEmpty();

      RuleFor(x => x)
        .HasRequiredScopes("event");
    }
  }

  public class QueryHandler : Handler<Query, IResponse<List<TagDto>>>
  {
    private readonly DynamoDBContext _client;

    public QueryHandler(IAmazonDynamoDB database)
    {
      _client = new DynamoDBContext(database);
    }

    public override async Task<IResponse<List<TagDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
      Logger.LogInformation($"Listing tags for account {request.AccountId}");

      var search = _client.FromQueryAsync<Tag>(
        new()
        {
          KeyExpression = new Expression
          {
            ExpressionStatement = "PartitionKey = :partitionKey",
            ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
            {
              { ":partitionKey", TagMapper.GetPartitionKey(request.AccountId!) },
            },
          },
          BackwardSearch = true,
        },
        new()
        {
          OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
        });
      var tags = await search.GetRemainingAsync(cancellationToken);

      Logger.LogInformation($"Found {tags.Count} tag(s)");

      return Response(tags.Select(x => TagMapper.ToDto(x)).ToList());
    }
  }
}
