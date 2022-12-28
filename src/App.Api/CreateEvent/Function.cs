using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using App.Api.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace App.Api.CreateEvent;

public class Function : FunctionBase
{
    private static readonly IServiceProvider _serviceProvider = Startup
        .ConfigureServices()
        .BuildServiceProvider();

    public Function() : base (_serviceProvider) { }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
        APIGatewayProxyRequest apiGatewayProxyRequest,
        ILambdaContext context)
    {
        var command = JsonSerializer.Deserialize<CreateEventCommand.Command>(apiGatewayProxyRequest.Body);

        return await Respond(command);
    }
}