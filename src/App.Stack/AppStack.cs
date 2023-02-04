using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Experimental;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using AppStack.Constructs;
using Constructs;
using static Amazon.CDK.AWS.CloudFront.CfnDistribution;

namespace AppStack;

public class AppStack : Stack
{
  internal AppStack(Construct scope, string id, IStackProps props)
    : base(scope, id, props)
  {
    // DynamoDB
    var tableName = "what-did-i-do";
    var applicationTable = CreateTable(tableName);

    // API Gateway
    var apiGateway = new RestApi(this, "what-did-i-do", new RestApiProps
    {
      RestApiName = "what-did-i-do",
    });

    // Resource: Login
    var loginResource = apiGateway.Root.AddResource("login");
    HandleLoginResource(loginResource, tableName);

    // Resource: Account
    var accountResource = apiGateway.Root.AddResource("account");
    HandleAccountResource(accountResource, tableName, applicationTable);

    // Resource: Event
    var eventResource = apiGateway.Root.AddResource("event");
    HandleEventResource(eventResource, tableName, applicationTable);

    // S3: Client
    var cloudFrontOriginAccessPrincipal = new OriginAccessIdentity(
      this, "CloudFrontOAI", new OriginAccessIdentityProps
      {
        Comment = "Allows CloudFront access to S3 bucket",
      });
    var clientBucket = CreateClientBucket(cloudFrontOriginAccessPrincipal);

    // Redirect NotFound Paths
    var redirectFunction = new EdgeFunction(this, "Redirect", new EdgeFunctionProps
    {
      Code = Code.FromAsset("./src/App.Stack/Redirect"),
      Handler = "index.handler",
      Runtime = Runtime.NODEJS_18_X,
    });

    // CloudFront Distribution
    var cloudFrontDistribution = CreateCloudFrontWebDistribution(
      apiGateway, clientBucket, cloudFrontOriginAccessPrincipal, redirectFunction);

    // Output
    new CfnOutput(this, "APIGWEndpoint", new CfnOutputProps
    {
      Value = apiGateway.Url,
    });
    new CfnOutput(this, "CloudFrontDomainName", new CfnOutputProps
    {
      Value = cloudFrontDistribution.DistributionDomainName,
    });
  }

  private Table CreateTable(string tableName)
    => new Table(this, "ApplicationTable", new TableProps
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

  private void HandleLoginResource(
    Amazon.CDK.AWS.APIGateway.Resource loginResource, string tableName)
  {
    var loginFunction = new AppFunction(this, "App.Login", new AppFunction.Props(
      "App.Login::App.Login.LambdaEntryPoint::FunctionHandlerAsync",
      tableName
    ));
    loginResource.AddProxy(new ProxyResourceOptions
    {
      AnyMethod = true,
      DefaultIntegration = new LambdaIntegration(loginFunction),
    });
  }

  private void HandleAccountResource(
    Amazon.CDK.AWS.APIGateway.Resource accountResource,
    string tableName,
    Table applicationTable)
  {
    // Create
    var createAccountFunction = new AppFunction(this, "CreateAccount", new AppFunction.Props(
      "CreateAccount::App.Api.CreateAccount.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadData(createAccountFunction);
    applicationTable.GrantWriteData(createAccountFunction);
    accountResource.AddMethod("POST", new LambdaIntegration(createAccountFunction));
  }

  private void HandleEventResource(
    Amazon.CDK.AWS.APIGateway.Resource eventResource,
    string tableName,
    Table applicationTable)
  {
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
  }

  private Bucket CreateClientBucket(OriginAccessIdentity cloudFrontOriginAccessPrincipal)
  {
    var clientBucket = new Bucket(this, "Client", new BucketProps
    {
      AccessControl = BucketAccessControl.PRIVATE,
      Cors = new[]
      {
        new CorsRule
        {
          AllowedOrigins = new[] { "*" },
          AllowedMethods = new[] { HttpMethods.GET },
          MaxAge = 3000,
        },
      },
    });
    new BucketDeployment(this, "DeployClient", new BucketDeploymentProps
    {
      Sources = new[] { Source.Asset("./src/App.Client/build") },
      DestinationBucket = clientBucket,
    });
    var policyStatement = new PolicyStatement(new PolicyStatementProps
    {
      Actions = new[] { "s3:GetObject" },
      Resources = new[] { $"{clientBucket.BucketArn}/*" },
    });
    policyStatement.AddCanonicalUserPrincipal(
      cloudFrontOriginAccessPrincipal.CloudFrontOriginAccessIdentityS3CanonicalUserId);
    clientBucket.AddToResourcePolicy(policyStatement);

    return clientBucket;
  }

  private CloudFrontWebDistribution CreateCloudFrontWebDistribution(
    RestApi apiGateway,
    Bucket clientBucket,
    OriginAccessIdentity cloudFrontOriginAccessPrincipal,
    EdgeFunction redirectFunction)
  {
    return new CloudFrontWebDistribution(
      this, "WhatDidIDoDistribution", new CloudFrontWebDistributionProps
      {
        DefaultRootObject = "index.html",
        ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
        PriceClass = PriceClass.PRICE_CLASS_ALL,
        OriginConfigs = new[]
        {
          new SourceConfiguration
          {
            CustomOriginSource = new CustomOriginConfig
            {
              DomainName = $"{apiGateway.RestApiId}.execute-api.{this.Region}.{this.UrlSuffix}",
              OriginPath = $"/${apiGateway.DeploymentStage.StageName}",
            },
            Behaviors = new[]
            {
              new Behavior
              {
                PathPattern = "/api/*",
                AllowedMethods = CloudFrontAllowedMethods.ALL,
                DefaultTtl = Duration.Seconds(0),
                ForwardedValues = new ForwardedValuesProperty
                {
                  QueryString = true,
                  Headers = new[] { "Authorization" },
                },
              },
            },
          },
          new SourceConfiguration
          {
            S3OriginSource = new S3OriginConfig
            {
              S3BucketSource = clientBucket,
              OriginAccessIdentity = cloudFrontOriginAccessPrincipal,
            },
            Behaviors = new[]
            {
              new Behavior
              {
                Compress = true,
                IsDefaultBehavior = true,
                DefaultTtl = Duration.Seconds(0),
                AllowedMethods = CloudFrontAllowedMethods.GET_HEAD_OPTIONS,
                LambdaFunctionAssociations = new[]
                {
                  new LambdaFunctionAssociation
                  {
                    LambdaFunction = redirectFunction.CurrentVersion,
                    EventType = LambdaEdgeEventType.ORIGIN_RESPONSE,
                  },
                },
              },
            },
          },
        },
      });
  }
}
