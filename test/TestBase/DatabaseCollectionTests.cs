using Amazon.DynamoDBv2;

namespace TestBase;

[Collection(Constants.DatabaseCollection)]
public class DatabaseCollectionTests
{
  [Fact]
  public async Task Should_CreateTestTable_When_DatabaseCollectionIsUsed()
  {
    var client = new AmazonDynamoDBClient();
    var table = await client.DescribeTableAsync(Environment.GetEnvironmentVariable("TABLE_NAME"));

    Assert.NotNull(table);
  }
}
