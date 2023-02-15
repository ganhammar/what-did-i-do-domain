using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
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
    var apiResource = apiGateway.Root.AddResource("api");

    // Resource: Login
    var loginResource = apiResource.AddResource("login");
    HandleLoginResource(loginResource, tableName);

    // Resource: Account
    var accountResource = apiResource.AddResource("account");
    HandleAccountResource(accountResource, tableName, applicationTable);

    // Resource: Event
    var eventResource = apiResource.AddResource("event");
    HandleEventResource(eventResource, tableName, applicationTable);

    // S3: Client
    var cloudFrontOriginAccessPrincipal = new OriginAccessIdentity(
      this, "CloudFrontOAI", new OriginAccessIdentityProps
      {
        Comment = "Allows CloudFront access to S3 bucket",
      });
    var clientBucket = CreateClientBucket(cloudFrontOriginAccessPrincipal);

    // CloudFront Distribution
    var cloudFrontDistribution = CreateCloudFrontWebDistribution(
      apiGateway, clientBucket, cloudFrontOriginAccessPrincipal);

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
      tableName,
      512
    ));

    var identityTable = Table.FromTableAttributes(this, "IdentityTable", new TableAttributes
    {
      TableArn = $"arn:aws:dynamodb:{this.Region}:{this.Account}:table/what-did-i-do.identity",
      GrantIndexPermissions = true,
    });
    identityTable.GrantReadWriteData(loginFunction);

    var openiddictTable = Table.FromTableAttributes(this, "OpenIddictTable", new TableAttributes
    {
      TableArn = $"arn:aws:dynamodb:{this.Region}:{this.Account}:table/what-did-i-do.openiddict",
      GrantIndexPermissions = true,
    });
    openiddictTable.GrantReadWriteData(loginFunction);

    var mailPolicy = new PolicyStatement(new PolicyStatementProps
    {
      Effect = Effect.ALLOW,
      Actions = new[]
      {
        "ses:SendEmail",
        "ses:SendRawEmail",
        "ses:SendTemplatedEmail",
      },
      Resources = new[]
      {
        "*",
      },
    });
    loginFunction.AddToRolePolicy(mailPolicy);

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
    applicationTable.GrantReadWriteData(createAccountFunction);
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
    applicationTable.GrantReadWriteData(createEventFunction);
    eventResource.AddMethod("POST", new LambdaIntegration(createEventFunction));

    // Delete
    var deleteEventFunction = new AppFunction(this, "DeleteEvent", new AppFunction.Props(
      "DeleteEvent::App.Api.DeleteEvent.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadWriteData(deleteEventFunction);
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
    OriginAccessIdentity cloudFrontOriginAccessPrincipal)
  {
    var certificate = Certificate.FromCertificateArn(
      this,
      "CustomDomainCertificate",
      "arn:aws:acm:us-east-1:519157272275:certificate/9ee8b722-7f87-41cc-85d6-968ea8e89eda");

    var distribution = new CloudFrontWebDistribution(
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
              OriginPath = $"/{apiGateway.DeploymentStage.StageName}",
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
              },
            },
          },
        },
        ErrorConfigurations = new[]
        {
          new CustomErrorResponseProperty
          {
            ErrorCode = 403,
            ResponsePagePath = "/",
            ErrorCachingMinTtl = 300,
            ResponseCode = 200,
          },
        },
        ViewerCertificate = ViewerCertificate.FromAcmCertificate(certificate),
      });

    CreateRecords(distribution);

    return distribution;
  }

  public void CreateRecords(CloudFrontWebDistribution distribution)
  {
    var zone = HostedZone.FromHostedZoneAttributes(this, "HostedZone", new HostedZoneAttributes
    {
      HostedZoneId = "Z03051521S1HVOKUZOI9I",
      ZoneName = "wdid.fyi",
    });

    new AaaaRecord(this, "AliasRecord", new AaaaRecordProps
    {
      Target = RecordTarget.FromAlias(new CloudFrontTarget(distribution)),
      Zone = zone,
    });

    new CnameRecord(this, "CnameRecord", new CnameRecordProps
    {
      RecordName = "www.wdid.fyi",
      DomainName = distribution.DistributionDomainName,
      Ttl = Duration.Minutes(5),
      Zone = zone,
    });
  }
}
