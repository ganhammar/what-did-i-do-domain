using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using App.Api.Shared.Infrastructure;
using App.Api.Shared.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Api.ListEvents;

public class Startup
{
  public static IConfiguration Configuration = FunctionConfiguration.Get();

  public static IServiceCollection ConfigureServices()
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
    services.AddMediatR(Assembly.GetCallingAssembly());
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipeline<,>));

    services.AddTransient<IResponse<List<EventDto>>, Response<List<EventDto>>>();
    services.AddTransient<IRequestHandler<ListEventsCommand.Command, IResponse<List<EventDto>>>, ListEventsCommand.CommandHandler>();
    services.AddTransient<IValidator<ListEventsCommand.Command>, ListEventsCommand.CommandValidator>();

    return services;
  }
}
