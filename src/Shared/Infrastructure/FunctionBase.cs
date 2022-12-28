using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Api.Shared.Infrastructure;

public abstract class FunctionBase
{
    private readonly IServiceProvider _serviceProvider;

    public FunctionBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> Respond<T>(IRequest<IResponse<T>>? request)
    {
        if (request == default)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonSerializer.Serialize(new[]
                {
                    new ValidationFailure("Body", "Invalid request"),
                }),
                StatusCode = (int)HttpStatusCode.BadRequest,
            };
        }

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(request);

        if (response.IsValid)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(response.Result),
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };
        }

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(response.Errors),
            Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
        };
    }
}