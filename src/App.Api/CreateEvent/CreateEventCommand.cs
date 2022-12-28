using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using FluentValidation;
using MediatR;

namespace App.Api.CreateEvent;

public class CreateEventCommand
{
    public class Command : IRequest<IResponse<Event>>
    {
        public string? Title { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty();
        }
    }

    public class CommandHandler : Handler<Command, IResponse<Event>>
    {
        public override Task<IResponse<Event>> Handle(Command request, CancellationToken cancellationToken)
        {
            IResponse<Event> response = new Response<Event>
            {
                Result = new Event
                {
                    Title = request.Title,
                },
            };
            return Task.FromResult(response);
        }
    }
}