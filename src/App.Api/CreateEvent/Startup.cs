using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using App.Api.Shared.Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<ApiGatewayProxyJsonSerializerContext>))]

namespace App.Api.CreateEvent;

[LambdaStartup]
public class Startup
{
    public IConfiguration Configuration = FunctionConfiguration.Get();

    public void ConfigureServices(IServiceCollection services)
    {
        AWSSDKHandler.RegisterXRayForAllServices();

        var dynamoDbConfig = Configuration.GetSection("DynamoDB");

        services
            .AddDefaultAWSOptions(Configuration.GetAWSOptions())
            .AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(new AmazonDynamoDBConfig
            {
                ServiceURL = dynamoDbConfig["ServiceUrl"],
            }));
        services.AddMediatR(Assembly.GetCallingAssembly());
    }
}