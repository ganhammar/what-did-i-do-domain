using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Api.Shared.Extensions;
using App.Api.Shared.Models;
using TestBase;
using TestBase.Helpers;
using Xunit;

namespace CreateAccountTests;

[Collection(Constants.DatabaseCollection)]
public class AccountMapperTests
{
  public async Task Should_AddSuffixToId_When_NameIsAlreadyTaken()
  {
    var name = "Microsoft";
    AccountHelpers.CreateAccount(new()
    {
      Id = name.UrlFriendly(),
      Name = name,
      CreateDate = DateTime.UtcNow,
    });

    var client = new AmazonDynamoDBClient();
    var dbContext = new DynamoDBContext(client);
    var id = await AccountMapper.GetUniqueId(name, dbContext, CancellationToken.None);

    Assert.Equal($"{name}-1", id);
  }
}
