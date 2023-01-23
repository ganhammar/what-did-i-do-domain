using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using App.Api.CreateEvent;
using App.Api.Shared.Models;
using FluentValidation.Results;
using TestBase;

namespace CreateEventTests;

[Collection(Constants.DatabaseCollection)]
public class FunctionTests
{
  [Fact]
  public async Task Should_ReturnEvent_When_InputIsValid()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new CreateEventCommand.Command
    {
      AccountId = Guid.NewGuid(),
      Title = "Testing Testing",
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    var body = JsonSerializer.Deserialize<EventDto>(response.Body);

    Assert.NotNull(body);
    Assert.Equal(data.Title, body!.Title);
    Assert.NotNull(body!.Id);
  }

  [Fact]
  public async Task Should_ReturnEvent_When_InputIsValidWithDate()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var date = DateTime.Now.AddDays(-1337).ToUniversalTime();
    var data = new CreateEventCommand.Command
    {
      AccountId = Guid.NewGuid(),
      Title = "Testing Testing",
      Date = date,
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    var body = JsonSerializer.Deserialize<EventDto>(response.Body);

    Assert.NotNull(body);
    Assert.Equal(data.Title, body!.Title);
    Assert.NotNull(body!.Id);
    Assert.Equal(date, body!.Date);
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_TitleIsNotSet()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new CreateEventCommand.Command
    {
      AccountId = Guid.NewGuid(),
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(CreateEventCommand.Command.Title)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_AccountIdIsNotSet()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new CreateEventCommand.Command
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
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(CreateEventCommand.Command.AccountId)
      && error.ErrorCode == "NotEmptyValidator");
  }
}
