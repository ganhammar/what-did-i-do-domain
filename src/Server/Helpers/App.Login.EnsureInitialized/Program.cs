using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using App.Login.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.AmazonDynamoDB;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var configuration = new ConfigurationBuilder()
  .AddJsonFile($"appsettings.json")
  .AddJsonFile($"appsettings.{environment}.json", optional: true)
  .AddEnvironmentVariables()
  .Build();

var dynamoDbConfig = configuration.GetSection("DynamoDB");
var serviceUrl = dynamoDbConfig.GetValue<string>("ServiceUrl");

Console.WriteLine($"Environment: {environment}");
Console.WriteLine($"ServiceURL: {dynamoDbConfig.GetValue<string>("ServiceUrl")}");

var services = new ServiceCollection();
var client = new AmazonDynamoDBClient(
  String.IsNullOrEmpty(serviceUrl) == false ? new AmazonDynamoDBConfig
  {
    ServiceURL = serviceUrl,
  } : new());

services.AddIdentityCore<DynamoDbUser>().AddRoles<DynamoDbRole>().AddDynamoDbStores().Configure(options =>
{
  options.DefaultTableName = "what-did-i-do.identity";
});
services.AddSingleton<IAmazonDynamoDB>(client);
services.AddOpenIddict().AddCore().UseDynamoDb().Configure(options =>
{
  options.DefaultTableName = "what-did-i-do.openiddict";
});

var serviceProvider = services.BuildServiceProvider();

AspNetCoreIdentityDynamoDbSetup.EnsureInitialized(serviceProvider);
OpenIddictDynamoDbSetup.EnsureInitialized(serviceProvider);

if (environment?.ToLower().Equals("development") == true)
{
  var tableName = "what-did-i-do";
  var exist = client.DescribeTableAsync(tableName).GetAwaiter().GetResult();

  if (exist.Table == default)
  {
    client.CreateTableAsync(new CreateTableRequest
    {
      BillingMode = BillingMode.PAY_PER_REQUEST,
      TableName = tableName,
      KeySchema = new List<KeySchemaElement>
      {
        new("PartitionKey", KeyType.HASH),
        new("SortKey", KeyType.RANGE),
      },
      AttributeDefinitions = new List<AttributeDefinition>
      {
        new("PartitionKey", ScalarAttributeType.S),
        new("SortKey", ScalarAttributeType.S),
      },
    }).GetAwaiter().GetResult();
  }
}

var tables = client.ListTablesAsync().GetAwaiter().GetResult();

Console.WriteLine("Tables initialized, the following tables exists:");

tables.TableNames.ForEach(tableName => Console.WriteLine(tableName));

var internalClients = new[] { "what-did-i-do.account" };

foreach (var clientId in internalClients)
{
  var applicationStore = serviceProvider.GetRequiredService<OpenIddictDynamoDbApplicationStore<OpenIddictDynamoDbApplication>>();

  var application = applicationStore.FindByClientIdAsync(clientId, CancellationToken.None).GetAwaiter().GetResult();

  if (application == default)
  {
    var clientSecret = Guid.NewGuid().ToString();

    Console.WriteLine($"Creating client with id \"{clientId}\" ({clientSecret})");

    application = new()
    {
      ClientId = clientId,
      ClientSecret = clientSecret,
      DisplayName = clientId,
    };

    applicationStore.CreateAsync(application, CancellationToken.None).GetAwaiter().GetResult();
  }
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("All good!");
Console.ResetColor();
