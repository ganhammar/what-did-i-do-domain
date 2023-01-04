using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
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
    }
  }

  public class CommandHandler : Handler<Command, IResponse<AccountDto>>
  {
    private readonly DynamoDBContext _client;

    public CommandHandler(IAmazonDynamoDB database)
    {
      _client = new DynamoDBContext(database);
    }

    public override async Task<IResponse<AccountDto>> Handle(Command request, CancellationToken cancellationToken)
    {
      var item = AccountMapper.FromDto(new AccountDto
      {
        Id = await AccountMapper.GetUniqueId(request.Name!, _client, cancellationToken),
        Name = request.Name,
        CreateDate = DateTime.UtcNow,
      });
      await _client.SaveAsync(item, new()
      {
        OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
      }, cancellationToken);

      return Response(AccountMapper.ToDto(item));
    }
  }
}
