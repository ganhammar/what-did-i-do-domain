using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Experimental;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
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

    // Authorizer
    var authorizerFunction = new AppFunction(this, "App.Authorizer", new AppFunction.Props(
      "App.Authorizer::App.Authorizer.Function::FunctionHandler"
    ));
    var authorizer = new RequestAuthorizer(this, "ApiAuthorizer", new RequestAuthorizerProps
    {
      Handler = authorizerFunction,
      IdentitySources = new[] { IdentitySource.Header("authorization") },
      ResultsCacheTtl = Duration.Seconds(0),
    });

    // Resource: Login
    var loginResource = apiResource.AddResource("login");
    HandleLoginResource(loginResource, tableName);

    // Resource: Account
    var accountResource = apiResource.AddResource("account");
    HandleAccountResource(accountResource, tableName, applicationTable, authorizer);

    // Resource: Event
    var eventResource = apiResource.AddResource("event");
    HandleEventResource(eventResource, tableName, applicationTable, authorizer);

    // CloudFront Distribution
    var cloudFrontDistribution = CreateCloudFrontWebDistribution(apiGateway);

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
      768
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

    AllowSes(loginFunction);
    AllowSsm(loginFunction);

    loginResource.AddProxy(new ProxyResourceOptions
    {
      AnyMethod = true,
      DefaultIntegration = new LambdaIntegration(loginFunction),
    });
  }

  private void AllowSsm(AppFunction function)
  {
    var ssmPolicy = new PolicyStatement(new PolicyStatementProps
    {
      Effect = Effect.ALLOW,
      Actions = new[]
      {
        "ssm:PutParameter",
        "ssm:GetParametersByPath",
      },
      Resources = new[]
      {
        "*",
      },
    });
    function.AddToRolePolicy(ssmPolicy);
  }

  private void AllowSes(AppFunction function)
  {
    var sesPolicy = new PolicyStatement(new PolicyStatementProps
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
    function.AddToRolePolicy(sesPolicy);
  }

  private void HandleAccountResource(
    Amazon.CDK.AWS.APIGateway.Resource accountResource,
    string tableName,
    Table applicationTable,
    RequestAuthorizer authorizer)
  {
    // Create
    var createAccountFunction = new AppFunction(this, "CreateAccount", new AppFunction.Props(
      "CreateAccount::App.Api.CreateAccount.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadWriteData(createAccountFunction);
    accountResource.AddMethod("POST", new LambdaIntegration(createAccountFunction), new MethodOptions
    {
      AuthorizationType = AuthorizationType.CUSTOM,
      Authorizer = authorizer,
    });

    // List
    var listAccountsFunction = new AppFunction(this, "ListAccounts", new AppFunction.Props(
      "CreateAccount::App.Api.CreateAccount.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadWriteData(listAccountsFunction);
    accountResource.AddMethod("GET", new LambdaIntegration(listAccountsFunction), new MethodOptions
    {
      AuthorizationType = AuthorizationType.CUSTOM,
      Authorizer = authorizer,
    });
  }

  private void HandleEventResource(
    Amazon.CDK.AWS.APIGateway.Resource eventResource,
    string tableName,
    Table applicationTable,
    RequestAuthorizer authorizer)
  {
    // Create
    var createEventFunction = new AppFunction(this, "CreateEvent", new AppFunction.Props(
      "CreateEvent::App.Api.CreateEvent.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadWriteData(createEventFunction);
    eventResource.AddMethod("POST", new LambdaIntegration(createEventFunction), new MethodOptions
    {
      AuthorizationType = AuthorizationType.CUSTOM,
      Authorizer = authorizer,
    });

    // Delete
    var deleteEventFunction = new AppFunction(this, "DeleteEvent", new AppFunction.Props(
      "DeleteEvent::App.Api.DeleteEvent.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadWriteData(deleteEventFunction);
    eventResource.AddMethod("DELETE", new LambdaIntegration(deleteEventFunction), new MethodOptions
    {
      AuthorizationType = AuthorizationType.CUSTOM,
      Authorizer = authorizer,
    });

    // List
    var listEventsFunction = new AppFunction(this, "ListEvents", new AppFunction.Props(
      "ListEvents::App.Api.ListEvents.Function::FunctionHandler",
      tableName
    ));
    applicationTable.GrantReadData(listEventsFunction);
    eventResource.AddMethod("GET", new LambdaIntegration(listEventsFunction), new MethodOptions
    {
      AuthorizationType = AuthorizationType.CUSTOM,
      Authorizer = authorizer,
    });
  }

  private Bucket CreateClientBucket(
    OriginAccessIdentity cloudFrontOriginAccessPrincipal,
    string name,
    string packagePath,
    string s3Path = "/")
  {
    var clientBucket = new Bucket(this, name, new BucketProps
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
    new BucketDeployment(this, $"Deploy{name}", new BucketDeploymentProps
    {
      Sources = new[] { Source.Asset($"./{packagePath}/build") },
      DestinationBucket = clientBucket,
      DestinationKeyPrefix = s3Path,
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
    RestApi apiGateway)
  {
    // Redirect NotFound Paths
    var routerFunction = new EdgeFunction(this, "Router", new EdgeFunctionProps
    {
      Code = Code.FromAsset("./src/App.Stack/Router"),
      Handler = "index.handler",
      Runtime = Runtime.NODEJS_18_X,
    });

    // Add X-Forwarded-Host Header
    var forwardedForFunction = new EdgeFunction(this, "ForwardedHostHeader", new EdgeFunctionProps
    {
      Code = Code.FromAsset("./src/App.Stack/ForwardedHostHeader"),
      Handler = "index.handler",
      Runtime = Runtime.NODEJS_18_X,
    });

    // S3: Login
    var loginPrincipal = new OriginAccessIdentity(
      this, "LoginCloudFrontOAI", new OriginAccessIdentityProps
      {
        Comment = "Allows CloudFront access to S3 bucket",
      });
    var loginBucket = CreateClientBucket(loginPrincipal, "Login", "login", "login");

    // S3: Account
    var accountPrincipal = new OriginAccessIdentity(
      this, "AccountCloudFrontOAI", new OriginAccessIdentityProps
      {
        Comment = "Allows CloudFront access to S3 bucket",
      });
    var accountBucket = CreateClientBucket(accountPrincipal, "Account", "account", "account");

    // S3: Landing
    var landingPrincipal = new OriginAccessIdentity(
      this, "LandingCloudFrontOAI", new OriginAccessIdentityProps
      {
        Comment = "Allows CloudFront access to S3 bucket",
      });
    var landingBucket = CreateClientBucket(landingPrincipal, "Landing", "landing");

    var certificate = Certificate.FromCertificateArn(
      this,
      "CustomDomainCertificate",
      "arn:aws:acm:us-east-1:519157272275:certificate/80bb1e7c-410d-4af5-82b0-ed061cd65271");

    var distribution = new CloudFrontWebDistribution(
      this, "WhatDidIDoDistribution", new CloudFrontWebDistributionProps
      {
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
                  Cookies = new CookiesProperty
                  {
                    Forward = "all",
                  },
                },
                LambdaFunctionAssociations = new[]
                {
                  new LambdaFunctionAssociation
                  {
                    LambdaFunction = forwardedForFunction.CurrentVersion,
                    EventType = LambdaEdgeEventType.VIEWER_REQUEST,
                  },
                },
              },
            },
          },
          new SourceConfiguration
          {
            S3OriginSource = new S3OriginConfig
            {
              S3BucketSource = loginBucket,
              OriginAccessIdentity = loginPrincipal,
            },
            Behaviors = new[]
            {
              new Behavior
              {
                PathPattern = "/login*",
                Compress = true,
                DefaultTtl = Duration.Seconds(0),
                AllowedMethods = CloudFrontAllowedMethods.GET_HEAD_OPTIONS,
                LambdaFunctionAssociations = new[]
                {
                  new LambdaFunctionAssociation
                  {
                    LambdaFunction = routerFunction.CurrentVersion,
                    EventType = LambdaEdgeEventType.ORIGIN_REQUEST,
                  },
                },
              },
            },
          },
          new SourceConfiguration
          {
            S3OriginSource = new S3OriginConfig
            {
              S3BucketSource = accountBucket,
              OriginAccessIdentity = accountPrincipal,
            },
            Behaviors = new[]
            {
              new Behavior
              {
                PathPattern = "/account*",
                Compress = true,
                DefaultTtl = Duration.Seconds(0),
                AllowedMethods = CloudFrontAllowedMethods.GET_HEAD_OPTIONS,
                LambdaFunctionAssociations = new[]
                {
                  new LambdaFunctionAssociation
                  {
                    LambdaFunction = routerFunction.CurrentVersion,
                    EventType = LambdaEdgeEventType.ORIGIN_REQUEST,
                  },
                },
              },
            },
          },
          new SourceConfiguration
          {
            S3OriginSource = new S3OriginConfig
            {
              S3BucketSource = landingBucket,
              OriginAccessIdentity = landingPrincipal,
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
                    LambdaFunction = routerFunction.CurrentVersion,
                    EventType = LambdaEdgeEventType.ORIGIN_REQUEST,
                  },
                },
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
        ViewerCertificate = ViewerCertificate.FromAcmCertificate(certificate, new ViewerCertificateOptions
        {
          Aliases = new[]
          {
            "wdid.fyi",
            "www.wdid.fyi",
          },
        }),
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
