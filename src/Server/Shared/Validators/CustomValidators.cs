using FluentValidation;

namespace App.Api.Shared.Validators;

public static class CustomValidators
{
  public static IRuleBuilderOptions<T, object?> HasRequiredScopes<T>(this IRuleBuilder<T, object?> ruleBuilder, params string[] requiredScopes)
    => ruleBuilder.SetValidator(new HasRequiredScopesValidator<T>(requiredScopes));
}
