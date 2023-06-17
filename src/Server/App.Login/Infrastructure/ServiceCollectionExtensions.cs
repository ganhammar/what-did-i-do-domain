using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;

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
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
      })
      .AddDefaultTokenProviders()
      .AddDynamoDbStores()
      .SetDefaultTableName("what-did-i-do.identity");

    services
      .Configure<IdentityOptions>(options =>
      {
        options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
        options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
        options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
        options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
      });
  }

  public static void AddOpenIddict(this IServiceCollection services, bool isDevelopment)
  {
    services
      .AddOpenIddict()
      .AddCore(builder =>
      {
        builder
          .UseDynamoDb()
          .SetDefaultTableName("what-did-i-do.openiddict");
      })
      .AddServer(builder =>
      {
        builder
          .SetAuthorizationEndpointUris($"{Constants.BasePath}/connect/authorize")
          .SetLogoutEndpointUris($"{Constants.BasePath}/connect/logout")
          .SetIntrospectionEndpointUris($"{Constants.BasePath}/connect/introspect")
          .SetUserinfoEndpointUris($"{Constants.BasePath}/connect/userinfo")
          .SetTokenEndpointUris($"{Constants.BasePath}/connect/token")
          .SetIntrospectionEndpointUris($"{Constants.BasePath}/connect/introspect")
          .SetCryptographyEndpointUris($"{Constants.BasePath}/.well-known/jwks")
          .SetConfigurationEndpointUris($"{Constants.BasePath}/.well-known/openid-configuration");

        builder.AllowImplicitFlow();
        builder.AllowRefreshTokenFlow();
        builder.AllowClientCredentialsFlow();
        builder.AllowAuthorizationCodeFlow();

        builder.UseReferenceAccessTokens();
        builder.UseReferenceRefreshTokens();

        builder.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

        builder.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
        builder.SetRefreshTokenLifetime(TimeSpan.FromDays(1));

        if (isDevelopment)
        {
          builder
            .AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        }
        else
        {
          builder
            .AddSigningCertificate(new X509Certificate2("./signing-certificate.pfx"))
            .AddEncryptionCertificate(new X509Certificate2("./encryption-certificate.pfx"));
        }

        var aspNetCoreBuilder = builder
          .UseAspNetCore()
          .EnableAuthorizationEndpointPassthrough()
          .EnableLogoutEndpointPassthrough()
          .EnableUserinfoEndpointPassthrough()
          .EnableStatusCodePagesIntegration()
          .EnableTokenEndpointPassthrough();

        if (isDevelopment)
        {
          aspNetCoreBuilder.DisableTransportSecurityRequirement();
        }
      })
      .AddValidation(builder =>
      {
        builder.UseLocalServer();
        builder.UseAspNetCore();
      });
  }

  public static void AddMediatR(this IServiceCollection services)
  {
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipeline<,>));
    services.AddMediatR(Assembly.GetAssembly(typeof(Startup))!);

    services.Scan(x => x
      .FromAssemblyOf<IResponse>()
        .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
          .AsImplementedInterfaces()
          .WithTransientLifetime()
        .AddClasses(classes => classes.AssignableTo(typeof(IResponse<>)))
          .AsImplementedInterfaces()
          .WithTransientLifetime()
        .AddClasses(classes => classes.AssignableTo<IResponse>())
          .AsImplementedInterfaces()
          .WithTransientLifetime()
        .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
          .AsImplementedInterfaces()
          .WithTransientLifetime()
        .AddClasses(classes => classes.AssignableTo(typeof(Handler<>)))
          .AsImplementedInterfaces()
          .WithTransientLifetime()
        .AddClasses(classes => classes.AssignableTo(typeof(Handler<,>)))
          .AsImplementedInterfaces()
          .WithTransientLifetime());
  }
}
