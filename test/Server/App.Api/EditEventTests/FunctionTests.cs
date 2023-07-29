using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using App.Api.EditEvent;
using App.Api.Shared.Models;
using FluentValidation.Results;
using TestBase;
using TestBase.Helpers;

namespace EditEventTests;

[Collection(Constants.DatabaseCollection)]
public class FunctionTests
{
  [Fact]
  public async Task Should_ReturnEvent_When_InputIsValid()
  {
    var item = EventMapper.ToDto(EventHelpers.CreateEvent(new()
    {
      AccountId = Guid.NewGuid().ToString(),
      Date = DateTime.UtcNow,
      Title = "test",
    }));
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new EditEventCommand.Command
    {
      Id = item.Id,
      Title = "Testing Testing",
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

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    var body = JsonSerializer.Deserialize<EventDto>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(body);
    Assert.Equal(data.Title, body!.Title);
  }

  [Fact]
  public async Task Should_ReturnEventWithTags_When_InputIsValid()
  {
    var item = EventMapper.ToDto(EventHelpers.CreateEvent(new()
    {
      AccountId = Guid.NewGuid().ToString(),
      Date = DateTime.UtcNow,
      Title = "test",
    }));
    var function = new Function();
    var context = new TestLambdaContext();
    var tags = new[] { "test", "testing" };
    var data = new EditEventCommand.Command
    {
      Id = item.Id,
      Title = "Testing Testing",
      Tags = tags,
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

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    var body = JsonSerializer.Deserialize<EventDto>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(body);
    Assert.Equal(data.Title, body!.Title);
    Assert.NotNull(body.Tags);
    Assert.Equal(2, body.Tags.Count());
    Assert.Contains(body.Tags, (tag) => tags.Contains(tag));
  }

  [Fact]
  public async Task Should_RemoveOldTags_When_InputContainsDuplicateNoLongerContainsTag()
  {
    var item = EventMapper.ToDto(EventHelpers.CreateEvent(new()
    {
      AccountId = Guid.NewGuid().ToString(),
      Date = DateTime.UtcNow,
      Title = "test",
      Tags = new[] { "test", "testing" },
    }));
    var function = new Function();
    var context = new TestLambdaContext();
    var tags = new[] { "test" };
    var data = new EditEventCommand.Command
    {
      Id = item.Id,
      Title = "Testing Testing",
      Tags = tags,
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

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    var body = JsonSerializer.Deserialize<EventDto>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(body);
    Assert.Equal(data.Title, body!.Title);
    Assert.NotNull(body.Tags);
    Assert.Single(body.Tags);
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_TitleIsNotSet()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new EditEventCommand.Command
    {
      Id = "test-event",
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
    Assert.Contains(errors, error => error.PropertyName == nameof(EditEventCommand.Command.Title)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_IdIsNotSet()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new EditEventCommand.Command
    {
      Title = "Testing Testing",
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
    Assert.Contains(errors, error => error.PropertyName == nameof(EditEventCommand.Command.Id)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnUnauthorized_When_RequiredScopeIsMissing()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new EditEventCommand.Command
    {
      Title = "Testing Testing",
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
