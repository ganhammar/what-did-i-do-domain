using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
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
        }
    }

    public class CommandHandler : Handler<Command, IResponse>
    {
        private readonly DynamoDBContext _client;

        public CommandHandler(IAmazonDynamoDB database)
        {
            _client = new DynamoDBContext(database);
        }

        public override async Task<IResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
            var keys = EventMapper.GetKeys(request.Id);
            var item  = await _client.LoadAsync<Event>(keys[0], keys[1], new()
            {
                OverrideTableName = tableName,
            }, cancellationToken);

            if (item != default)
            {
                await _client.DeleteAsync(item, new()
                {
                    OverrideTableName = tableName,
                }, cancellationToken);
            } 

            return Response();
        }
    }
}