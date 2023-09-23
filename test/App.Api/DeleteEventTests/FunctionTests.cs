using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
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
    var data = new Dictionary<string, string>
    {
      { "Id", EventMapper.ToDto(item).Id! },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      QueryStringParameters = data,
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
  public async Task Should_DeleteTags_When_InputIsValid()
  {
    var accountId = Guid.NewGuid().ToString();
    var date = DateTime.UtcNow;
    var tags = new[] { "test", "testing" };
    var item = EventHelpers.CreateEvent(new()
    {
      AccountId = accountId,
      Title = "Testing Testing",
      Date = date,
      Tags = tags,
    });

    var context = new TestLambdaContext();
    var data = new Dictionary<string, string>
    {
      { "Id", EventMapper.ToDto(item).Id! },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      QueryStringParameters = data,
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

    var client = new AmazonDynamoDBClient();
    var dbContext = new DynamoDBContext(client);
    var batch = dbContext.CreateBatchGet<EventTag>(new()
    {
      OverrideTableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
    });

    foreach (var tag in tags)
    {
      var eventTagDto = EventTagMapper.FromDto(new()
      {
        AccountId = accountId,
        Date = date,
        Value = tag,
      });
      batch.AddKey(eventTagDto.PartitionKey, eventTagDto.SortKey);
    }

    await batch.ExecuteAsync();

    Assert.Empty(batch.Results);
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_InputIsNotValid()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      QueryStringParameters = new Dictionary<string, string>(),
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
    var data = new Dictionary<string, string>
    {
      { "Id", "not-the-real-deal" },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      QueryStringParameters = data,
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
    var data = new Dictionary<string, string>
    {
      { "Id", EventMapper.ToDto(item).Id! },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      QueryStringParameters = data,
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
