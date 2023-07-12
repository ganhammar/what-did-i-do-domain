using Amazon.DynamoDBv2;
using App.Login.Features.Email;
using App.Login.Infrastructure;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Logging;

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
  private const string AllowLocalhost = "_allowLocalhost";

  public void ConfigureServices(IServiceCollection services)
  {
    var dynamoDbConfig = Configuration.GetSection("DynamoDB");
    var serviceUrl = dynamoDbConfig.GetValue<string>("ServiceUrl");

    services.Configure<ForwardedHeadersOptions>(options =>
    {
      options.ForwardedHeaders = ForwardedHeaders.XForwardedHost;
    });

    services
      .AddDefaultAWSOptions(Configuration.GetAWSOptions())
      .AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(
        string.IsNullOrEmpty(serviceUrl) == false ? new AmazonDynamoDBConfig
        {
          ServiceURL = dynamoDbConfig.GetValue<string>("ServiceUrl"),
        } : new()));

    services.AddDataProtection()
      .PersistKeysToAWSSystemsManager("/WhatDidIDo/DataProtection");

    services.AddIdentity();
    services.AddCors(options =>
    {
      options.AddPolicy(name: AllowLocalhost,
        policy =>
        {
          policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
    });
    services.AddOpenIddict(Configuration, Environment.IsDevelopment());
    services.AddMediatR();
    services.AddSingleton<IEmailSender, EmailSender>();

    services
      .ConfigureApplicationCookie(options =>
      {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.Cookie.Path = "/";
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
    services
      .AddControllers()
      .AddFeatureFolders();
    services.AddHealthChecks();
  }

  public void Configure(
    IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
  {
    app.UseForwardedHeaders();

    app.Use(async (context, next) =>
    {
      logger.LogInformation($"Incoming request to {context.Request.Path}");

      if (context.Request.PathBase.HasValue)
      {
        logger.LogInformation($"Request has a path base, {context.Request.PathBase.Value}, replacing with empty path");
        context.Request.PathBase = PathString.Empty;
      }

      await next(context);
    });

    if (Environment.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }
    else
    {
      app.UseStatusCodePagesWithReExecute("/error");
    }

    app.UseCors(AllowLocalhost);
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
