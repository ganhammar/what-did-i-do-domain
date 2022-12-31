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
using Xunit;

namespace CreateEventTests;

[Collection(Constants.DatabaseCollection)]
public class FunctionTests
{
  [Fact]
  public async Task Should_ReturnSuccessfully_When_InputIsValid()
  {
    var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
    var item = EventMapper.FromDto(new EventDto
    {
      Date = DateTime.UtcNow,
      Title = "Testing Testing",
    });
    var client = new AmazonDynamoDBClient();
    var dbContext = new DynamoDBContext(client);
    dbContext.SaveAsync(item, new()
    {
      OverrideTableName = tableName,
    }, CancellationToken.None).GetAwaiter().GetResult();

    var context = new TestLambdaContext();
    var data = new DeleteEventCommand.Command
    {
      Id = EventMapper.ToDto(item).Id,
    };
    var request = new APIGatewayProxyRequest
    {
      HttpMethod = HttpMethod.Post.Method,
      Body = JsonSerializer.Serialize(data),
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
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

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
    };
    var response = await function.FunctionHandler(request, context);

    Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);

    var errors = JsonSerializer.Deserialize<List<ValidationFailure>>(response.Body);

    Assert.NotNull(errors);
    Assert.Contains(errors, error => error.PropertyName == nameof(DeleteEventCommand.Command.Id)
      && error.ErrorCode == DeleteEventCommand.InvalidId);
  }
}
