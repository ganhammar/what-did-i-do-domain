using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace App.Api.Shared.Infrastructure;

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class ApiGatewayProxyJsonSerializerContext : JsonSerializerContext
{
}
