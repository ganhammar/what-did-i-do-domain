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

var serviceProvider = services.BuildServiceProvider();

AspNetCoreIdentityDynamoDbSetup.EnsureInitialized(serviceProvider);
OpenIddictDynamoDbSetup.EnsureInitialized(serviceProvider);

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

var tables = client.ListTablesAsync().GetAwaiter().GetResult();

Console.WriteLine("Tables initialized, the following tables exists:");

tables.TableNames.ForEach(tableName => Console.WriteLine(tableName));

var clientOptions = serviceProvider.GetRequiredService<IOptionsMonitor<ClientOptions>>();

if (clientOptions.CurrentValue.Clients?.Any() == true)
{
  foreach (var internalClient in clientOptions.CurrentValue.Clients)
  {
    ArgumentNullException.ThrowIfNull(internalClient.Id);
    ArgumentNullException.ThrowIfNull(internalClient.Secret);

    var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    var application = (OpenIddictDynamoDbApplication?)applicationManager
      .FindByClientIdAsync(internalClient.Id, CancellationToken.None).GetAwaiter().GetResult();

    if (application == default)
    {
      Console.WriteLine($"Attempting to create client with id \"{internalClient.Id}\"");

      var descriptor = new OpenIddictApplicationDescriptor
      {
        ClientId = internalClient.Id,
        ClientSecret = internalClient.Secret,
        DisplayName = internalClient.Id,
        Permissions =
        {
          OpenIddictConstants.Permissions.Endpoints.Token,
          OpenIddictConstants.Permissions.Endpoints.Introspection,
          OpenIddictConstants.Permissions.Endpoints.Authorization,
          OpenIddictConstants.Permissions.Endpoints.Logout,
          OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
        },
      };

      if (!isDevelopment)
      {
        applicationManager.CreateAsync(descriptor, CancellationToken.None).GetAwaiter().GetResult();
        Console.WriteLine("Client created");
      }
      else
      {
        Console.WriteLine("Skipping create client in development");
      }
    }
  }
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("All good!");
Console.ResetColor();
