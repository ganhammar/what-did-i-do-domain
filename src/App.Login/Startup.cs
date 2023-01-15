using Amazon.DynamoDBv2;
using App.Login.Infrastructure;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.IdentityModel.Logging;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace App.Login;

public class Startup
{
  public Startup(IConfiguration configuration, IHostEnvironment environment)
  {
    Configuration = configuration;
    Environment = environment;
  }

  public IConfiguration Configuration { get; }
  public IHostEnvironment Environment { get; }

  public void ConfigureServices(IServiceCollection services)
  {
    var dynamoDbConfig = Configuration.GetSection("DynamoDB");

    services
      .AddDefaultAWSOptions(Configuration.GetAWSOptions())
      .AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(new AmazonDynamoDBConfig
      {
        ServiceURL = dynamoDbConfig.GetValue<string>("ServiceUrl"),
      }));

    services.AddIdentity();
    services.AddCors();

    services
      .AddOpenIddict()
      .AddCore(builder =>
      {
        builder.UseDynamoDb();
      })
      .AddServer(builder =>
      {
        builder
          .SetAuthorizationEndpointUris("/connect/authorize")
          .SetLogoutEndpointUris("/connect/logout")
          .SetIntrospectionEndpointUris("/connect/introspect")
          .SetUserinfoEndpointUris("/connect/userinfo")
          .SetTokenEndpointUris("/connect/token");

        builder.AllowImplicitFlow();
        builder.AllowRefreshTokenFlow();
        builder.AllowClientCredentialsFlow();
        builder.AllowAuthorizationCodeFlow();

        builder.UseReferenceAccessTokens();
        builder.UseReferenceRefreshTokens();

        builder.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

        builder.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
        builder.SetRefreshTokenLifetime(TimeSpan.FromDays(1));

        // TODO: Fix production certificates
        // if (Environment.IsDevelopment())
        // {
        builder
          .AddDevelopmentEncryptionCertificate()
          .AddDevelopmentSigningCertificate();
        // }

        var aspNetCoreBuilder = builder
          .UseAspNetCore()
          .EnableAuthorizationEndpointPassthrough()
          .EnableLogoutEndpointPassthrough()
          .EnableUserinfoEndpointPassthrough()
          .EnableStatusCodePagesIntegration()
          .EnableTokenEndpointPassthrough();

        if (Environment.IsDevelopment())
        {
          aspNetCoreBuilder.DisableTransportSecurityRequirement();
        }
      })
      .AddValidation(builder =>
      {
        builder.UseLocalServer();
        builder.UseAspNetCore();
      });

    services
      .ConfigureApplicationCookie(options =>
      {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
      })
      .Configure<CookiePolicyOptions>(options =>
      {
        options.Secure = CookieSecurePolicy.Always;
        options.HttpOnly = HttpOnlyPolicy.Always;
      });

    if (Environment.IsDevelopment())
    {
      IdentityModelEventSource.ShowPII = true;
    }

    services.AddHttpContextAccessor();
    services.AddControllers();
    services.AddHealthChecks();
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    app.UseForwardedHeaders();

    if (Environment.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }
    else
    {
      app.UseStatusCodePagesWithReExecute("/error");
    }

    app.UseCors();
    app.UseCookiePolicy();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseEndpoints(options =>
    {
      options.MapControllers();
      options.MapDefaultControllerRoute();
      options.MapHealthChecks("/health");
      options.MapFallbackToFile("index.html");
    });
  }
}
