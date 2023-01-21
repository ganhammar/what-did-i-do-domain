using Amazon.DynamoDBv2;
using App.Login.Infrastructure;
using AspNetCore.Identity.AmazonDynamoDB;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.AmazonDynamoDB;

var services = new ServiceCollection();
var client = new AmazonDynamoDBClient();

services.AddIdentity();
services.AddOpenIddict(true);
services.AddSingleton<IAmazonDynamoDB>(client);

var serviceProvider = services.BuildServiceProvider();

AspNetCoreIdentityDynamoDbSetup.EnsureInitialized(serviceProvider);
OpenIddictDynamoDbSetup.EnsureInitialized(serviceProvider);
