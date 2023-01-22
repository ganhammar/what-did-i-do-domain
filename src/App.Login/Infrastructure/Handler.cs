using FluentValidation.Results;
using MediatR;

namespace App.Login.Infrastructure;

public abstract class Handler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
{
  public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

  protected virtual Response<TResult> Response<TResult>(TResult result)
      where TResult : new()
  {
    var response = new Response<TResult>
    {
      Result = result
    };

    return response;
  }

  protected virtual Response<TResult> Response<TResult>(TResult result, IEnumerable<ValidationFailure> errors)
      where TResult : new()
  {
    var response = new Response<TResult>
    {
      Result = result,
      Errors = errors,
    };

    return response;
  }

  protected virtual Response Response(IEnumerable<ValidationFailure> errors)
  {
    var response = new Response
    {
      Errors = errors,
    };

    return response;
  }

  protected virtual Response Response()
  {
    var response = new Response();

    return response;
  }
}

public abstract class Handler<TRequest> : IRequestHandler<TRequest>
    where TRequest : IRequest
{
  async Task<Unit> IRequestHandler<TRequest, Unit>.Handle(TRequest request, CancellationToken cancellationToken)
  {
    await Handle(request, cancellationToken).ConfigureAwait(false);
    return Unit.Value;
  }

  protected abstract Task Handle(TRequest request, CancellationToken cancellationToken);
}
