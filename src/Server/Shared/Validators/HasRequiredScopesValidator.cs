using App.Api.Shared.Infrastructure;
using FluentValidation;
using FluentValidation.Validators;

namespace App.Api.Shared.Validators;

public class HasRequiredScopesValidator<T> : PropertyValidator<T, object?>
{
  private readonly List<string> _requiredScopes;

  public HasRequiredScopesValidator(params string[] requiredScopes)
  {
    _requiredScopes = requiredScopes.ToList();
  }

  public override string Name => "UnauthorizedRequest";

  public override bool IsValid(ValidationContext<T> context, object? value)
  {
    if (_requiredScopes.Any())
    {
      if (APIGatewayProxyRequestAccessor.Current!.RequestContext.Authorizer.TryGetValue("scope", out var scopes) == false)
      {
        return false;
      }

      if (string.IsNullOrEmpty(scopes.ToString()) || _requiredScopes.Except(scopes.ToString()!.Split(" ")).Any() == true)
      {
        return false;
      }
    }

    return true;
  }

  protected override string GetDefaultMessageTemplate(string errorCode)
      => "User not authorized to perform this request";
}
