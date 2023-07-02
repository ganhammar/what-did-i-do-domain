using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace TestBase;

public class DatabaseFixture : IDisposable
{
  private bool _disposed;
  private readonly string _tableName = Guid.NewGuid().ToString();
  private readonly AmazonDynamoDBClient _client;

  public DatabaseFixture()
  {
    SetEnvironment();

    _client = new AmazonDynamoDBClient();

    CreateTable().GetAwaiter().GetResult();
    var tables = _client.ListTablesAsync().GetAwaiter().GetResult();
  }

  private void SetEnvironment()
  {
    Environment.SetEnvironmentVariable("TABLE_NAME", _tableName);
  }

  private async Task CreateTable()
  {
    await _client.CreateTableAsync(new CreateTableRequest
    {
      TableName = _tableName,
      BillingMode = BillingMode.PAY_PER_REQUEST,
      KeySchema = new()
      {
        new("PartitionKey", KeyType.HASH),
        new("SortKey", KeyType.RANGE),
      },
      AttributeDefinitions = new()
      {
        new("PartitionKey", ScalarAttributeType.S),
        new("SortKey", ScalarAttributeType.S),
        new("Subject", ScalarAttributeType.S),
      },
      GlobalSecondaryIndexes = new()
      {
        new()
        {
          IndexName = "Subject-index",
          KeySchema = new List<KeySchemaElement>
          {
            new KeySchemaElement("Subject", KeyType.HASH),
            new KeySchemaElement("PartitionKey", KeyType.RANGE),
          },
          Projection = new Projection
          {
            ProjectionType = ProjectionType.ALL,
          },
        },
      },
    });

    var created = false;
    while (!created)
    {
      var table = await _client.DescribeTableAsync(_tableName);

      if (table.Table.TableStatus == TableStatus.ACTIVE)
      {
        created = true;
      }

      await Task.Delay(TimeSpan.FromSeconds(1));
    }
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _client.DeleteTableAsync(_tableName).GetAwaiter().GetResult();
    _client.Dispose();
    _disposed = true;
  }
}
