using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using App.Api.Shared.Models;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace App.Api.CreateEvent;

public class Function
{
    private static readonly IServiceProvider _serviceProvider = Startup
        .ConfigureServices()
        .BuildServiceProvider();

    public Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
        APIGatewayHttpApiV2ProxyRequest apigProxyEvent,
        ILambdaContext context)
    {
        if (!apigProxyEvent.RequestContext.Http.Method.Equals(HttpMethod.Post.Method))
        {
            return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "Only POST allowed",
                StatusCode = (int)HttpStatusCode.MethodNotAllowed,
            });
        }

        try
        {
            var data = JsonSerializer.Deserialize<Event>(apigProxyEvent.Body);

            return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(data),
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            });
        }
        catch (Exception exception)
        {
            context.Logger.LogLine($"Error creating product {exception.Message} {exception.StackTrace}");

            return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "Something Went Wrong",
                StatusCode = (int)HttpStatusCode.InternalServerError,
            });
        }
    }
}