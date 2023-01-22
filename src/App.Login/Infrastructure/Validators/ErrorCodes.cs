namespace App.Login.Infrastructure.Validators;

public static class ErrorCodes
{
  public const string EmailUnconfirmed = "The email is not confirmed";
  public const string NoLoginAttemptInProgress = "No login request is in progress";
  public const string TwoFactorProviderNotValid = "The selected two factor provider is not valid in the current context";
}
