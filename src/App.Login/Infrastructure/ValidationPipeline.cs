using FluentValidation;
using MediatR;

namespace App.Login.Infrastructure;

public class ValidationPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResponse
{
  private readonly IEnumerable<IValidator<TRequest>> _validators;
  private TResponse _response;

  public ValidationPipeline(IEnumerable<IValidator<TRequest>> validators, TResponse response)
  {
    _validators = validators;
    _response = response;
  }

  public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    var failures = _validators
        .Select(v => v.ValidateAsync(request).Result)
        .SelectMany(result => result.Errors)
        .Where(f => f != null)
        .ToList();

    if (failures.Any())
    {
      _response.Errors = failures;
      return Task.FromResult(_response);
    }

    return next();
  }
}
