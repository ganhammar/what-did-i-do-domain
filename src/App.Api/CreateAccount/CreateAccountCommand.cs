using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Extensions;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using App.Api.Shared.Validators;
using AWS.Lambda.Powertools.Logging;
using FluentValidation;
using MediatR;

namespace App.Api.CreateAccount;

public class CreateAccountCommand
{
  public class Command : IRequest<IResponse<AccountDto>>
  {
    public string? Name { get; set; }
  }

  public class CommandValidator : AbstractValidator<Command>
  {
    public CommandValidator()
    {
      RuleFor(x => x.Name)
        .NotEmpty();

      RuleFor(x => x)
        .HasRequiredScopes("account");
    }
  }

  public class CommandHandler : Handler<Command, IResponse<AccountDto>>
  {
    private readonly DynamoDBContext _client;

    public CommandHandler(IAmazonDynamoDB database)
    {
      _client = new DynamoDBContext(database);
    }

    public override async Task<IResponse<AccountDto>> Handle(
      Command request, CancellationToken cancellationToken)
    {
      Logger.LogInformation("Attempting to create account");

      var id = await AccountMapper.GetUniqueId(request.Name!, _client, cancellationToken);
      Logger.LogInformation($"The unique Id for the account is {id}");

      var config = new DynamoDBOperationConfig
      {
        OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
      };

      var item = AccountMapper.FromDto(new AccountDto
      {
        Id = id,
        Name = request.Name,
        CreateDate = DateTime.UtcNow,
      });
      await _client.SaveAsync(item, config, cancellationToken);
      Logger.LogInformation("Account created");

      var member = MemberMapper.FromDto(new MemberDto
      {
        AccountId = id,
        Role = Role.Owner,
        Subject = APIGatewayProxyRequestAccessor.Current?.GetSubject(),
        Email = APIGatewayProxyRequestAccessor.Current?.GetEmail(),
        CreateDate = DateTime.UtcNow,
      });

      ArgumentNullException.ThrowIfNull(member.Subject, nameof(member.Subject));
      ArgumentNullException.ThrowIfNull(member.Email, nameof(member.Email));

      Logger.LogInformation("Attempting to create account member of type owner");
      await _client.SaveAsync(member, config, cancellationToken);

      Logger.LogInformation("Member created");
      return Response(AccountMapper.ToDto(item));
    }
  }
}
