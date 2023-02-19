using System.Security.Claims;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace App.Login.Tests.Infrastructure;

public class MockSignInManager : SignInManager<DynamoDbUser>
{
  public MockSignInManager(
    UserManager<DynamoDbUser> userManager,
    IHttpContextAccessor contextAccessor,
    IUserClaimsPrincipalFactory<DynamoDbUser> claimsFactory,
    IOptions<IdentityOptions> optionsAccessor,
    ILogger<SignInManager<DynamoDbUser>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<DynamoDbUser> confirmation)
      : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
  {
  }

  public DynamoDbUser? CurrentUser { get; set; }
  private bool _signInRequetInProgress = false;

  public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
  {
    if (userName == "valid@wdid.fyi" && password == "itsaseasyas123")
    {
      _signInRequetInProgress = true;
      CurrentUser = await FindFirstUser();
      return SignInResult.Success;
    }

    return SignInResult.Failed;
  }

  public override async Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberClient)
  {
    var user = await GetTwoFactorAuthenticationUserAsync();
    if (user == default)
    {
      return SignInResult.Failed;
    }

    var result = await UserManager.VerifyTwoFactorTokenAsync(user, provider, code);
    if (result)
    {
      CurrentUser = user;
      return SignInResult.Success;
    }

    CurrentUser = default;
    return SignInResult.Failed;
  }

  public override async Task<DynamoDbUser> GetTwoFactorAuthenticationUserAsync()
  {
    if (!_signInRequetInProgress)
    {
      return default!;
    }

    CurrentUser = await FindFirstUser();
    return CurrentUser;
  }

  public override Task<ClaimsPrincipal> CreateUserPrincipalAsync(DynamoDbUser user)
      => Task.FromResult(CreateClaimsPrincipal(user));

  public ClaimsPrincipal CreateClaimsPrincipal(DynamoDbUser user)
  {
    return new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
    {
      new Claim(Claims.Email, user.Email ?? ""),
      new Claim(Claims.Name, user.UserName ?? ""),
      new Claim(Claims.PhoneNumber, user.PhoneNumber ?? ""),
      new Claim(Claims.Subject, user.Id),
      new Claim(Claims.Private.Scope, Scopes.Email),
      new Claim(Claims.Private.Scope, Scopes.Phone),
      new Claim(Claims.Private.Scope, Scopes.Roles),
    }, "TestAuth"));
  }

  public override Task SignOutAsync()
  {
    return Task.CompletedTask;
  }

  private async Task<DynamoDbUser> FindFirstUser()
  {
    var database = base.Context.RequestServices.GetRequiredService<IAmazonDynamoDB>();
    var context = new DynamoDBContext(database);
    var scan = context.ScanAsync<DynamoDbUser>(new List<ScanCondition>());
    var users = await scan.GetNextSetAsync();
    return users.First();
  }
}
