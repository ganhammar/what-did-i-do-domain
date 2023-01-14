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

    var loginFunction = new AppFunction(this, "App.Login", new AppFunction.Props(
      "Login::App.Login.LambdaEntryPoint::FunctionHandlerAsync",
      tableName
    ));

    var createAccountFunction = new AppFunction(this, "CreateAccount", new AppFunction.Props(
      "CreateAccount::App.Api.CreateAccount.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadData(createAccountFunction);
    applicationTable.GrantWriteData(createAccountFunction);

    var createEventFunction = new AppFunction(this, "CreateEvent", new AppFunction.Props(
      "CreateEvent::App.Api.CreateEvent.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantWriteData(createEventFunction);

    var deleteEventFunction = new AppFunction(this, "DeleteEvent", new AppFunction.Props(
      "DeleteEvent::App.Api.DeleteEvent.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadData(deleteEventFunction);
    applicationTable.GrantWriteData(deleteEventFunction);

    var listEventsFunction = new AppFunction(this, "ListEvents", new AppFunction.Props(
      "ListEvents::App.Api.ListEvents.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadData(listEventsFunction);

    var apiGateway = new RestApi(this, "what-did-i-do", new RestApiProps
    {
      RestApiName = "what-did-i-do",
    });

    var accountResource = apiGateway.Root.AddResource("account");
    accountResource.AddMethod("POST", new LambdaIntegration(createAccountFunction));

    var eventResource = apiGateway.Root.AddResource("event");
    eventResource.AddMethod("POST", new LambdaIntegration(createEventFunction));
    eventResource.AddMethod("DELETE", new LambdaIntegration(deleteEventFunction));
    eventResource.AddMethod("GET", new LambdaIntegration(listEventsFunction));

    new CfnOutput(this, "APIGWEndpoint", new CfnOutputProps
    {
      Value = apiGateway.Url,
    });
  }
}
