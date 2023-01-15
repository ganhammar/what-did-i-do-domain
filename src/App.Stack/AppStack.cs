using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using AppStack.Constructs;
using Constructs;

namespace AppStack;

public class AppStack : Stack
{
  internal AppStack(Construct scope, string id, IStackProps props)
    : base(scope, id, props)
  {
    // DynamoDB
    var tableName = "what-did-i-do";
    var applicationTable = new Table(this, "ApplicationTable", new TableProps
    {
      TableName = tableName,
      RemovalPolicy = RemovalPolicy.DESTROY, //Delete DynamoDB table on CDK destroy
      PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
      {
        Name = "PartitionKey",
        Type = AttributeType.STRING,
      },
      SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
      {
        Name = "SortKey",
        Type = AttributeType.STRING,
      },
      PointInTimeRecovery = true,
      BillingMode = BillingMode.PAY_PER_REQUEST,
    });

    // API Gateway
    var apiGateway = new RestApi(this, "what-did-i-do", new RestApiProps
    {
      RestApiName = "what-did-i-do",
    });

    // Login
    var loginResource = apiGateway.Root.AddResource("login");
    var loginFunction = new AppFunction(this, "App.Login", new AppFunction.Props(
      "App.Login::App.Login.LambdaEntryPoint::FunctionHandlerAsync",
      tableName
    ));
    loginResource.AddProxy(new ProxyResourceOptions
    {
      AnyMethod = true,
      DefaultIntegration = new LambdaIntegration(loginFunction),
    });

    // Resource: Account
    var accountResource = apiGateway.Root.AddResource("account");

    // Create
    var createAccountFunction = new AppFunction(this, "CreateAccount", new AppFunction.Props(
      "CreateAccount::App.Api.CreateAccount.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadData(createAccountFunction);
    applicationTable.GrantWriteData(createAccountFunction);
    accountResource.AddMethod("POST", new LambdaIntegration(createAccountFunction));

    // Resource: Event
    var eventResource = apiGateway.Root.AddResource("event");

    // Create
    var createEventFunction = new AppFunction(this, "CreateEvent", new AppFunction.Props(
      "CreateEvent::App.Api.CreateEvent.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantWriteData(createEventFunction);
    eventResource.AddMethod("POST", new LambdaIntegration(createEventFunction));

    // Delete
    var deleteEventFunction = new AppFunction(this, "DeleteEvent", new AppFunction.Props(
      "DeleteEvent::App.Api.DeleteEvent.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadData(deleteEventFunction);
    applicationTable.GrantWriteData(deleteEventFunction);
    eventResource.AddMethod("DELETE", new LambdaIntegration(deleteEventFunction));

    // List
    var listEventsFunction = new AppFunction(this, "ListEvents", new AppFunction.Props(
      "ListEvents::App.Api.ListEvents.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadData(listEventsFunction);
    eventResource.AddMethod("GET", new LambdaIntegration(listEventsFunction));

    // Output
    new CfnOutput(this, "APIGWEndpoint", new CfnOutputProps
    {
      Value = apiGateway.Url,
    });
  }
}
