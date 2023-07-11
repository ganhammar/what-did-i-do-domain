﻿using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Logging;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Api.Shared.Infrastructure;

public abstract class FunctionBase
{
  protected readonly IServiceProvider ServiceProvider;
  protected readonly IConfiguration Configuration;
  protected string? SystemsManagerPath;
  private readonly APIGatewayHttpApiV2ProxyResponse _noBodyResponse = new APIGatewayHttpApiV2ProxyResponse
  {
    Body = JsonSerializer.Serialize(new[]
    {
      new ValidationFailure("Body", "Invalid request")
      {
        ErrorCode = "InvalidRequest",
      },
    }),
    StatusCode = (int)HttpStatusCode.BadRequest,
  };

  public FunctionBase()
  {
    Configuration = BuildConfiguration();
    ServiceProvider = BuildServiceProvider();
  }

  protected virtual void ConfigureServices(IServiceCollection services) { }

  private IConfiguration BuildConfiguration()
  {
    var configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: true)
      .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
      .AddEnvironmentVariables();

    if (string.IsNullOrEmpty(this.SystemsManagerPath) == false)
    {
      configuration.AddSystemsManager(this.SystemsManagerPath);
    }

    return configuration.Build();
  }

  private IServiceProvider BuildServiceProvider()
  {
    var services = new ServiceCollection();

    AWSSDKHandler.RegisterXRayForAllServices();
#if DEBUG
    AWSXRayRecorder.Instance.XRayOptions.IsXRayTracingDisabled = true;
#endif

    var dynamoDbConfig = Configuration.GetSection("DynamoDB");

    services
      .AddDefaultAWSOptions(Configuration.GetAWSOptions())
      .AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipeline<,>));

    ConfigureServices(services);

    return services.BuildServiceProvider();
  }

  protected T? TryDeserialize<T>(APIGatewayProxyRequest apiGatewayProxyRequest)
  {
    if (string.IsNullOrEmpty(apiGatewayProxyRequest.Body))
    {
      return default;
    }

    return JsonSerializer.Deserialize<T>(
      apiGatewayProxyRequest.Body,
      new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
      });
  }

  protected async Task<APIGatewayHttpApiV2ProxyResponse> Respond<T>(IRequest<IResponse<T>>? request)
  {
    if (request == default)
    {
      return _noBodyResponse;
    }

    var mediator = ServiceProvider.GetRequiredService<IMediator>();
    var response = await mediator.Send(request);

    if (response.IsValid)
    {
      return new APIGatewayHttpApiV2ProxyResponse
      {
        StatusCode = (int)HttpStatusCode.OK,
        Body = JsonSerializer.Serialize(response.Result),
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };
    }

    return HandleErrorResponse(response);
  }

  protected async Task<APIGatewayHttpApiV2ProxyResponse> Respond(IRequest<IResponse>? request)
  {
    if (request == default)
    {
      return _noBodyResponse;
    }

    var mediator = ServiceProvider.GetRequiredService<IMediator>();
    var response = await mediator.Send(request);

    if (response.IsValid)
    {
      return new APIGatewayHttpApiV2ProxyResponse
      {
        StatusCode = (int)HttpStatusCode.NoContent,
      };
    }

    return HandleErrorResponse(response);
  }

  protected void AppendLookup(APIGatewayProxyRequest apiGatewayProxyRequest)
  {
    var requestContextRequestId = apiGatewayProxyRequest.RequestContext.RequestId;
    AppendLookup(requestContextRequestId);
  }

  protected void AppendLookup(string lookupId)
  {
    var lookupInfo = new Dictionary<string, object>()
    {
      { "LookupInfo", new Dictionary<string, object>{{ "LookupId", lookupId }} },
    };
    Logger.AppendKeys(lookupInfo);
  }

  private APIGatewayHttpApiV2ProxyResponse HandleErrorResponse(IResponse response)
    => new APIGatewayHttpApiV2ProxyResponse
    {
      StatusCode = (int)HttpStatusCode.BadRequest,
      Body = JsonSerializer.Serialize(response.Errors),
      Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    };
}
