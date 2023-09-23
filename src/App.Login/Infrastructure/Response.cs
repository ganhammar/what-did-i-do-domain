using FluentValidation.Results;

namespace App.Login.Infrastructure;

public class Response<T> : Response, IResponse<T>
    where T : new()
{
  public T? Result
  {
    get => IsValid ? ResultObject : default;
    set
    {
      if (value != null)
      {
        ResultObject = value;
      }
    }
  }

  private T ResultObject { get; set; } = new T();
}

public class Response : IResponse
{
  public Response()
  {
    Errors = Enumerable.Empty<ValidationFailure>();
  }

  public bool IsValid
  {
    get => !Errors.Any();
  }

  public IEnumerable<ValidationFailure> Errors { get; set; }
}
