using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using App.Api.Shared.Models;

namespace App.Api.Shared.Infrastructure;

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Event))]
public partial class ApiGatewayProxyJsonSerializerContext : JsonSerializerContext
{
}
