using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
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

        var createEventFunction = new Function(this, "CreateEventFunction", new FunctionProps
        {
            Runtime = Runtime.DOTNET_6,
            Architecture = Architecture.ARM_64,
            Handler = "CreateEvent::App.Api.CreateEvent.Function::FunctionHandler",
            Code = Code.FromAsset("./.output/CreateEvent.zip"),
            Timeout = Duration.Minutes(1),
            MemorySize = 128,
            Environment = new Dictionary<string, string>
            {
                { "TABLE_NAME", tableName },
            }
        });
        applicationTable.GrantWriteData(createEventFunction);

        var apiGateway = new RestApi(this, "what-did-i-do", new RestApiProps
        {
            RestApiName = "what-did-i-do",
        });

        var eventResource = apiGateway.Root.AddResource("event");
        eventResource.AddMethod("POST", new LambdaIntegration(createEventFunction));

        new CfnOutput(this, "APIGWEndpoint", new CfnOutputProps
        {
            Value = apiGateway.Url,
        });
    }
}