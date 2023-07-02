using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using App.Api.ListEvents;
using App.Api.Shared.Models;
using FluentValidation.Results;
using TestBase;
using TestBase.Helpers;

namespace ListEventsTests;

[Collection(Constants.DatabaseCollection)]
public class FunctionTests
{
  [Fact]
  public async Task Should_ReturnList_When_InputIsValid()
  {
    var accountId = Guid.NewGuid().ToString();
    EventHelpers.CreateEvent(new()
    {
      AccountId = accountId,
      Title = "Testing Testing",
      Date = DateTime.UtcNow,
    });
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, string>
    {
      { "AccountId", accountId },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      QueryStringParameters = data,
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    var body = JsonSerializer.Deserialize<List<EventDto>>(response.Body);

    Assert.NotNull(body);
    Assert.Single(body);
    Assert.Equal(accountId, body.First().AccountId);
  }

  [Fact]
  public async Task Should_ReturnEmptyList_When_ThereIsNoEventsBetweenFromAndTo()
  {
    var accountId = Guid.NewGuid().ToString();
    EventHelpers.CreateEvent(new()
    {
      AccountId = accountId,
      Title = "Testing Testing",
      Date = DateTime.UtcNow,
    });
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, string>
    {
      { "AccountId", accountId },
      { "FromDate", DateTime.UtcNow.AddDays(-7).ToString() },
      { "ToDate", DateTime.UtcNow.AddDays(-6).ToString() },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      QueryStringParameters = data,
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    var body = JsonSerializer.Deserialize<List<EventDto>>(response.Body);

    Assert.NotNull(body);
    Assert.Empty(body);
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_AccountIdIsNotSet()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.AccountId)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_FromDateIsSetAndToDateIsnt()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, string>
    {
      { "AccountId", Guid.NewGuid().ToString() },
      { "FromDate", DateTime.UtcNow.ToString() },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      QueryStringParameters = data,
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.ToDate)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_ToDateIsSetAndFromDateIsnt()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, string>
    {
      { "AccountId", Guid.NewGuid().ToString() },
      { "ToDate", DateTime.UtcNow.ToString() },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      QueryStringParameters = data,
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.FromDate)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_ToDateIsLessThanFromDate()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, string>
    {
      { "AccountId", Guid.NewGuid().ToString() },
      { "ToDate", DateTime.UtcNow.AddDays(-1).ToString() },
      { "FromDate", DateTime.UtcNow.ToString() },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      QueryStringParameters = data,
      RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
      {
        RequestId = Guid.NewGuid().ToString(),
      },
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.FromDate)
      && error.ErrorCode == "LessThanValidator");
  }
}
