using System.Security.Cryptography.X509Certificates;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using App.Login.EnsureInitialized;
using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.AmazonDynamoDB;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var isDevelopment = environment.ToLower().Equals("development");
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
services
  .AddOpenIddict()
  .AddCore(builder =>
  {
    builder
      .UseDynamoDb()
      .Configure(builder =>
      {
        builder.DefaultTableName = "what-did-i-do.openiddict";
      });
  })
  .AddServer(builder =>
  {
    if (isDevelopment)
    {
      builder
        .AddDevelopmentEncryptionCertificate()
        .AddDevelopmentSigningCertificate();
    }
    else
    {
      builder
        .AddSigningCertificate(new X509Certificate2("./signing-certificate.pfx"))
        .AddEncryptionCertificate(new X509Certificate2("./encryption-certificate.pfx"));
    }
  });

var config = configuration.GetSection("ClientOptions");
services.Configure<ClientOptions>(configuration.GetSection(nameof(ClientOptions)));
services.Configure<ScopeOptions>(configuration.GetSection(nameof(ScopeOptions)));

var serviceProvider = services.BuildServiceProvider();

// Ensure Tables Is Created
AspNetCoreIdentityDynamoDbSetup.EnsureInitialized(serviceProvider);
OpenIddictDynamoDbSetup.EnsureInitialized(serviceProvider);

var tableName = "what-did-i-do";
var exists = false;

try
{
  var tableResponse = client.DescribeTableAsync(tableName).GetAwaiter().GetResult();

  if (tableResponse.Table != default)
  {
    exists = true;
  }
}
catch (Exception)
{
  Console.WriteLine($"Table \"{tableName}\" does not exists, attempting to create");
}

if (exists == false)
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
  }).GetAwaiter().GetResult();
}

var tables = client.ListTablesAsync().GetAwaiter().GetResult();

Console.WriteLine("Tables initialized, the following tables exists:");

tables.TableNames.ForEach(tableName => Console.WriteLine(tableName));

// Ensure Applications Is Created
var clientOptions = serviceProvider.GetRequiredService<IOptionsMonitor<ClientOptions>>();

if (clientOptions.CurrentValue.Clients?.Any() == true)
{
  var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

  foreach (var applicationDescriptor in clientOptions.CurrentValue.Clients)
  {
    ArgumentNullException.ThrowIfNull(applicationDescriptor.ClientId);

    var application = (OpenIddictDynamoDbApplication?)applicationManager
      .FindByClientIdAsync(applicationDescriptor.ClientId, CancellationToken.None).GetAwaiter().GetResult();

    if (application == default)
    {
      Console.WriteLine($"Attempting to create client with id \"{applicationDescriptor.ClientId}\"");

      if (!isDevelopment)
      {
        applicationManager.CreateAsync(applicationDescriptor, CancellationToken.None).GetAwaiter().GetResult();
        Console.WriteLine("Client created");
      }
      else
      {
        Console.WriteLine("Skipping create client in development");
      }
    }
  }
}

// Ensure Scopes Is Created
var scopeOptions = serviceProvider.GetRequiredService<IOptionsMonitor<ScopeOptions>>();

if (scopeOptions.CurrentValue.Scopes?.Any() == true)
{
  var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();

  foreach (var scopeDescriptor in scopeOptions.CurrentValue.Scopes)
  {
    ArgumentNullException.ThrowIfNull(scopeDescriptor.Name);

    if (await scopeManager.FindByNameAsync(scopeDescriptor.Name) == null)
    {
      Console.WriteLine($"Attempting to create scope with name \"{scopeDescriptor.Name}\"");

      if (!isDevelopment)
      {
        await scopeManager.CreateAsync(scopeDescriptor);
        Console.WriteLine("Scope created");
      }
      else
      {
        Console.WriteLine("Skipping create scope in development");
      }
    }
  }
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("All good!");
Console.ResetColor();
