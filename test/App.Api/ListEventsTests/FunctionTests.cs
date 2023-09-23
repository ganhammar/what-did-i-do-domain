using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using App.Api.ListEvents;
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
      { "Limit", "100" },
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

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    var body = JsonSerializer.Deserialize<Result>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(body);
    Assert.Single(body.Items);
    Assert.Equal(accountId, body.Items.First().AccountId);
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
      { "Limit", "100" },
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

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    var body = JsonSerializer.Deserialize<Result>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(body);
    Assert.Empty(body.Items);
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
    var data = new Dictionary<string, string>
    {
      { "AccountId", Guid.NewGuid().ToString() },
      { "FromDate", DateTime.UtcNow.ToString() },
      { "Limit", "100" },
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
      { "Limit", "100" },
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
      { "Limit", "100" },
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
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.FromDate)
      && error.ErrorCode == "LessThanValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_LimitIsNotSet()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, string>
    {
      { "AccountId", Guid.NewGuid().ToString() },
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
    var data = new Dictionary<string, string>
    {
      { "AccountId", Guid.NewGuid().ToString() },
      { "FromDate", DateTime.UtcNow.AddDays(-7).ToString() },
      { "ToDate", DateTime.UtcNow.AddDays(-6).ToString() },
      { "Limit", limit },
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
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsQuery.Query.Limit)
      && error.ErrorCode == expectedErrorCode);
  }

  [Fact]
  public async Task Should_ReturnUnauthorized_When_RequiredScopeIsMissing()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new Dictionary<string, string>
    {
      { "AccountId", Guid.NewGuid().ToString() },
      { "Limit", "100" },
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
    var data = new Dictionary<string, string>
    {
      { "AccountId", accountId },
      { "Limit", "100" },
      { "Tag", "test" },
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

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    var body = JsonSerializer.Deserialize<Result>(response.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(body);
    Assert.Single(body.Items);
    Assert.Equal(accountId, body.Items.First().AccountId);
    Assert.NotNull(body.Items.First().Tags);
    Assert.Single(body.Items.First().Tags!);
    Assert.Equal("test", body.Items.First()!.Tags!.First());
  }

  [Theory]
  [InlineData(default)]
  [InlineData("test")]
  [InlineData("testing")]
  public async Task Should_ReturnNextPage_When_CalledWithPaginationToken(string? tag)
  {
    var accountId = Guid.NewGuid().ToString();
    EventHelpers.CreateEvent(new()
    {
      AccountId = accountId,
      Title = "Testing Testing",
      Date = DateTime.UtcNow,
      Tags = new[] { "test", "testing" },
    });
    EventHelpers.CreateEvent(new()
    {
      AccountId = accountId,
      Title = "Testing 2 Testing 2",
      Date = DateTime.UtcNow,
      Tags = new[] { "test" },
    });
    EventHelpers.CreateEvent(new()
    {
      AccountId = accountId,
      Title = "Testing 3 Testing 3",
      Date = DateTime.UtcNow,
      Tags = new[] { "testing" },
    });

    var getResponse = async (Dictionary<string, string> data) =>
    {
      var function = new Function();
      var context = new TestLambdaContext();
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
      return await function.FunctionHandler(request, context);
    };

    var data = new Dictionary<string, string>
    {
      { "AccountId", accountId },
      { "Limit", "1" },
    };

    if (tag != default)
    {
      data.Add("Tag", tag);
    }

    var firstPage = await getResponse(data);

    Assert.Equal((int)HttpStatusCode.OK, firstPage.StatusCode);

    var firstPageBody = JsonSerializer.Deserialize<Result>(firstPage.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(firstPageBody);
    Assert.Single(firstPageBody.Items);
    Assert.NotNull(firstPageBody.PaginationToken);

    data.Add("PaginationToken", firstPageBody.PaginationToken);

    var secondPage = await getResponse(data);

    Assert.Equal((int)HttpStatusCode.OK, secondPage.StatusCode);

    var secondPageBody = JsonSerializer.Deserialize<Result>(secondPage.Body, new JsonSerializerOptions()
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.NotNull(secondPageBody);
    Assert.Single(secondPageBody.Items);
    Assert.NotEqual(firstPageBody.Items.First().Id, secondPageBody.Items.First().Id);
  }
}
