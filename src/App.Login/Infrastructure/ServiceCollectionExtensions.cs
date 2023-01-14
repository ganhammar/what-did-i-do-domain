using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace App.Login.Infrastructure;

public static class ServiceCollectionExtensions
{
  public static void AddIdentity(this IServiceCollection services)
  {
    services
      .AddIdentity<DynamoDbUser, DynamoDbRole>(options =>
      {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredUniqueChars = 3;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
      })
      .AddDefaultTokenProviders()
      .AddDynamoDbStores();

    services
      .Configure<IdentityOptions>(options =>
      {
        options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
        options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
        options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
        options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
      });
  }
}
