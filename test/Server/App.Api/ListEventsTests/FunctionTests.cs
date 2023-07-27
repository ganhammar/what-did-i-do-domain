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
    var data = new Dictionary<string, IList<string>>
    {
      { "AccountId", new List<string> { accountId } },
      { "Limit", new List<string> { "100" } },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      MultiValueQueryStringParameters = data,
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

    var body = JsonSerializer.Deserialize<List<EventDto>>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

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
    var data = new Dictionary<string, IList<string>>
    {
      { "AccountId", new List<string> { accountId } },
      { "FromDate", new List<string> { DateTime.UtcNow.AddDays(-7).ToString() } },
      { "ToDate", new List<string> { DateTime.UtcNow.AddDays(-6).ToString() } },
      { "Limit", new List<string> { "100" } },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      MultiValueQueryStringParameters = data,
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

    var body = JsonSerializer.Deserialize<List<EventDto>>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

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
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.AccountId)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_FromDateIsSetAndToDateIsnt()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, IList<string>>
    {
      { "AccountId", new List<string> { Guid.NewGuid().ToString() } },
      { "FromDate", new List<string> { DateTime.UtcNow.ToString() } },
      { "Limit", new List<string> { "100" } },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      MultiValueQueryStringParameters = data,
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
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.ToDate)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_ToDateIsSetAndFromDateIsnt()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, IList<string>>
    {
      { "AccountId", new List<string> { Guid.NewGuid().ToString() } },
      { "ToDate", new List<string> { DateTime.UtcNow.ToString() } },
      { "Limit", new List<string> { "100" } },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      MultiValueQueryStringParameters = data,
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
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.FromDate)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_ToDateIsLessThanFromDate()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, IList<string>>
    {
      { "AccountId", new List<string> { Guid.NewGuid().ToString() } },
      { "ToDate", new List<string> { DateTime.UtcNow.AddDays(-1).ToString() } },
      { "FromDate", new List<string> { DateTime.UtcNow.ToString() } },
      { "Limit", new List<string> { "100" } },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      MultiValueQueryStringParameters = data,
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
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.FromDate)
      && error.ErrorCode == "LessThanValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_LimitIsNotSet()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, IList<string>>
    {
      { "AccountId", new List<string> { Guid.NewGuid().ToString() } },
      { "FromDate", new List<string> { DateTime.UtcNow.AddDays(-7).ToString() } },
      { "ToDate", new List<string> { DateTime.UtcNow.AddDays(-6).ToString() } },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      MultiValueQueryStringParameters = data,
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
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.Limit)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Theory]
  [InlineData("-1", "GreaterThanValidator")]
  [InlineData("201", "LessThanOrEqualValidator")]
  [InlineData("NaN", "NotEmptyValidator")]
  public async Task Should_ReturnBadRequest_When_LimitIsInvalid(string limit, string expectedErrorCode)
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, IList<string>>
    {
      { "AccountId", new List<string> { Guid.NewGuid().ToString() } },
      { "FromDate", new List<string> { DateTime.UtcNow.AddDays(-7).ToString() } },
      { "ToDate", new List<string> { DateTime.UtcNow.AddDays(-6).ToString() } },
      { "Limit", new List<string> { limit } },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      MultiValueQueryStringParameters = data,
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
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.Limit)
      && error.ErrorCode == expectedErrorCode);
  }

  [Fact]
  public async Task Should_ReturnUnauthorized_When_RequiredScopeIsMissing()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, IList<string>>
    {
      { "AccountId", new List<string> { Guid.NewGuid().ToString() } },
      { "Limit", new List<string> { "100" } },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      MultiValueQueryStringParameters = data,
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

  [Fact]
  public async Task Should_FilterByTags_When_InputContainsTagsFilter()
  {
    var accountId = Guid.NewGuid().ToString();
    EventHelpers.CreateEvent(new()
    {
      AccountId = accountId,
      Title = "Testing Testing",
      Date = DateTime.UtcNow,
      Tags = new[] { "test" },
    });
    EventHelpers.CreateEvent(new()
    {
      AccountId = accountId,
      Title = "Testing 2 Testing 2",
      Date = DateTime.UtcNow,
      Tags = new[] { "testing" },
    });

    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, IList<string>>
    {
      { "AccountId", new List<string>() { accountId } },
      { "Limit", new List<string>() { "100" } },
      { "Tags", new List<string>() { "test" } },
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      MultiValueQueryStringParameters = data,
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

    var body = JsonSerializer.Deserialize<List<EventDto>>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(body);
    Assert.Single(body);
    Assert.Equal(accountId, body.First().AccountId);
    Assert.NotNull(body.First().Tags);
    Assert.Single(body.First().Tags!);
    Assert.Equal("test", body.First()!.Tags!.First());
  }
}
