using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using App.Api.DeleteEvent;
using App.Api.Shared.Models;
using FluentValidation.Results;
using TestBase;
using TestBase.Helpers;

namespace CreateEventTests;

[Collection(Constants.DatabaseCollection)]
public class FunctionTests
{
  [Fact]
  public async Task Should_ReturnSuccessfully_When_InputIsValid()
  {
    var item = EventHelpers.CreateEvent(new()
    {
      AccountId = Guid.NewGuid().ToString(),
      Title = "Testing Testing",
      Date = DateTime.UtcNow,
    });

    var context = new TestLambdaContext();
    var data = new DeleteEventCommand.Command
    {
      Id = EventMapper.ToDto(item).Id,
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
        Authorizer = new()
        {
          { "scope", "email test event" },
          { "sub", Guid.NewGuid() },
          { "email", "test@wdid.fyi" },
        },
      },
    };

    var function = new Function();
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.NoContent, response.StatusCode);
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_InputIsNotValid()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new DeleteEventCommand.Command();
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
        Authorizer = new()
        {
          { "scope", "email test event" },
          { "sub", Guid.NewGuid() },
          { "email", "test@wdid.fyi" },
        },
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(DeleteEventCommand.Command.Id)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_IdIsNotValid()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new DeleteEventCommand.Command
    {
      Id = "not-the-real-deal",
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
        Authorizer = new()
        {
          { "scope", "email test event" },
          { "sub", Guid.NewGuid() },
          { "email", "test@wdid.fyi" },
        },
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(DeleteEventCommand.Command.Id)
      && error.ErrorCode == DeleteEventCommand.InvalidId);
  }

  [Fact]
  public async Task Should_ReturnUnauthorized_When_RequiredScopeIsMissing()
  {
    var item = EventHelpers.CreateEvent(new()
    {
      AccountId = Guid.NewGuid().ToString(),
      Title = "Testing Testing",
      Date = DateTime.UtcNow,
    });

    var function = new Function();
    var context = new TestLambdaContext();
    var data = new DeleteEventCommand.Command
    {
      Id = EventMapper.ToDto(item).Id,
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
        Authorizer = new()
        {
          { "scope", "email test account" },
          { "sub", Guid.NewGuid() },
          { "email", "test@wdid.fyi" },
        },
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.ErrorCode == "UnauthorizedRequest");
  }
}
