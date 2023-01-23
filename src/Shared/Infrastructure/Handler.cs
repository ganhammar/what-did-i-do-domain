using MediatR;

namespace App.Api.Shared.Infrastructure;

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

  protected virtual Response Response()
  {
    var response = new Response();

    return response;
  }
}
