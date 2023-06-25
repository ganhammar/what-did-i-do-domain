namespace App.Authorizer;

public interface ITokenClient
{
  public Task<IntrospectionResult> Validate(AuthorizationOptions authorizationOptions, string token);
}
