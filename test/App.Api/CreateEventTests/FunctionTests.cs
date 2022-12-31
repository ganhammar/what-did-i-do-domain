using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using App.Api.CreateEvent;
using App.Api.Shared.Models;
using FluentValidation.Results;
using TestBase;
using Xunit;

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
      Title = "Testing Testing",
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    var body = JsonSerializer.Deserialize<EventDto>(response.Body);

    Assert.NotNull(body);
    Assert.Equal(data.Title, body!.Title);
  }

  [Fact]
  public async Task Should_ReturnBadRequest_When_InputIsNotValid()
  {
    var function = new Function();
    var context = new TestLambdaContext();
    var data = new CreateEventCommand.Command();
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(CreateEventCommand.Command.Title)
      && error.ErrorCode == "NotEmptyValidator");
  }
}
