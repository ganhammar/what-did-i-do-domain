using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using App.Api.Shared.Extensions;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using App.Api.Shared.Validators;
using AWS.Lambda.Powertools.Logging;
using FluentValidation;
using MediatR;

namespace App.Api.ListAccounts;

public class ListAccountsQuery
{
  public class Query : IRequest<IResponse<List<AccountDto>>>
  {
  }

  public class QueryValidator : AbstractValidator<Query>
  {
    public QueryValidator()
    {
      RuleFor(x => x)
        .HasRequiredScopes("account");
    }
  }

  public class QueryHandler : Handler<Query, IResponse<List<AccountDto>>>
  {
    private readonly DynamoDBContext _client;

    public QueryHandler(IAmazonDynamoDB database)
    {
      _client = new DynamoDBContext(database);
    }

    public override async Task<IResponse<List<AccountDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
      Logger.LogInformation("Listing accounts for logged in user");

      var subject = APIGatewayProxyRequestAccessor.Current!.GetSubject();

      ArgumentNullException.ThrowIfNull(subject, nameof(Member.Subject));

      var config = new DynamoDBOperationConfig
      {
        OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
      };
      var search = _client.FromQueryAsync<Member>(new()
      {
        IndexName = "Subject-index",
        KeyExpression = new Expression
        {
          ExpressionStatement = "Subject = :subject AND begins_with(PartitionKey, :partitionKey)",
          ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
          {
            { ":subject", subject },
            { ":partitionKey", "MEMBER#" },
          }
        },
      }, config);
      var memberAccounts = await search.GetRemainingAsync(cancellationToken);

      Logger.LogInformation($"User is member of {memberAccounts.Count} accounts");

      var batch = _client.CreateBatchGet<Account>(config);
      foreach (var accountId in memberAccounts.Select(x => MemberMapper.ToDto(x).AccountId).Distinct())
      {
        var account = AccountMapper.FromDto(new AccountDto
        {
          Id = accountId,
        });
        batch.AddKey(account.PartitionKey, account.SortKey);
      }
      await batch.ExecuteAsync(cancellationToken);

      Logger.LogInformation($"Fetched {batch.Results.Count} unique accounts");

      return Response(batch.Results.Select(x => AccountMapper.ToDto(x)).ToList());
    }
  }
}
