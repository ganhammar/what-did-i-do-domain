using Amazon.DynamoDBv2;
using Xunit;

namespace TestBase;

public class FunctionTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Should_CreateTestTable_When_DatabaseFixtureIsUsed()
    {
        var client = new AmazonDynamoDBClient(
            new AmazonDynamoDBConfig
            {
                ServiceURL = Environment.GetEnvironmentVariable("DynamoDB__ServiceUrl"),
            });
        var table = await client.DescribeTableAsync(Environment.GetEnvironmentVariable("TABLE_NAME"));

        Assert.NotNull(table);
    }
}