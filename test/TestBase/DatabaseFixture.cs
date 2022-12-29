using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace TestBase;

public class DatabaseFixture : IDisposable
{
    private bool _disposed;
    private readonly string _tableName = Guid.NewGuid().ToString();
    private readonly string _serviceUrlKey = "DynamoDB__ServiceUrl";
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

        if (Environment.GetEnvironmentVariable(_serviceUrlKey) == default) {
            Environment.SetEnvironmentVariable(_serviceUrlKey, "http://localhost:8000");
        }
    }

    private async Task CreateTable()
    {
        await _client.CreateTableAsync(new CreateTableRequest
        {
            TableName = _tableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "PartitionKey",
                    KeyType = KeyType.HASH,
                },
                new KeySchemaElement
                {
                    AttributeName = "SortKey",
                    KeyType = KeyType.RANGE,
                },
            },
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition
                {
                    AttributeName = "PartitionKey",
                    AttributeType = ScalarAttributeType.S,
                },
                new AttributeDefinition
                {
                    AttributeName = "SortKey",
                    AttributeType = ScalarAttributeType.S,
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
