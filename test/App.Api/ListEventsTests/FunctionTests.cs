using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using App.Api.ListEvents;
using App.Api.Shared.Models;
using FluentValidation.Results;
using TestBase;
using TestBase.Helpers;
using Xunit;

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
    var data = new ListEventsCommand.Command
    {
      AccountId = accountId,
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
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
    var data = new ListEventsCommand.Command
    {
      AccountId = accountId,
      FromDate = DateTime.UtcNow.AddDays(-7),
      ToDate = DateTime.UtcNow.AddDays(-6),
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
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
    var data = new ListEventsCommand.Command();
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsCommand.Command.AccountId)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_FromDateIsSetAndToDateIsnt()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new ListEventsCommand.Command
    {
      AccountId = Guid.NewGuid().ToString(),
      FromDate = DateTime.UtcNow,
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsCommand.Command.ToDate)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_ToDateIsSetAndFromDateIsnt()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new ListEventsCommand.Command
    {
      AccountId = Guid.NewGuid().ToString(),
      ToDate = DateTime.UtcNow,
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsCommand.Command.FromDate)
      && error.ErrorCode == "NotEmptyValidator");
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_ToDateIsLessThanFromDate()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new ListEventsCommand.Command
    {
      AccountId = Guid.NewGuid().ToString(),
      ToDate = DateTime.UtcNow.AddDays(-1),
      FromDate = DateTime.UtcNow,
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(ListEventsCommand.Command.FromDate)
      && error.ErrorCode == "LessThanValidator");
  }
}
