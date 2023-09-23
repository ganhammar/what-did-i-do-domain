using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Experimental;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SSM;
using AppStack.Constructs;
using Constructs;
using Microsoft.Extensions.Configuration;
using static Amazon.CDK.AWS.CloudFront.CfnDistribution;
using static Amazon.CDK.AWS.CloudFront.CfnOriginAccessControl;

namespace AppStack;

public class AppStack : Stack
{
  private const string _tableName = "what-did-i-do";
  private readonly IConfiguration _configuration;

  internal AppStack(Construct scope, string id, IStackProps props, IConfiguration configuration)
    : base(scope, id, props)
  {
    _configuration = configuration;

    // DynamoDB
    var applicationTable = CreateTable();

    // API Gateway
    var apiGateway = new RestApi(this, "what-did-i-do", new RestApiProps
    {
      RestApiName = "what-did-i-do",
      DefaultCorsPreflightOptions = new CorsOptions
      {
        AllowOrigins = new[]
        {
          "http://localhost:3000",
        },
      },
    });
    var apiResource = apiGateway.Root.AddResource("api");

    // Authorizer
    var authorizer = CreateAuthorizerFunction();

    // Resource: Login
    var loginResource = apiResource.AddResource("login");
    HandleLoginResource(loginResource);

    // Resource: Account
    var accountResource = apiResource.AddResource("account");
    HandleAccountResource(accountResource, applicationTable, authorizer);

    // Resource: Event
    var eventResource = apiResource.AddResource("event");
    HandleEventResource(eventResource, applicationTable, authorizer);

    // Resource: Tag
    var tagResource = apiResource.AddResource("tag");
    HandleTagResource(tagResource, applicationTable, authorizer);

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

  private Table CreateTable()
  {
    var table = new Table(this, "ApplicationTable", new TableProps
    {
      TableName = _tableName,
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

    table.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
    {
      IndexName = "Subject-index",
      PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
      {
        Name = "Subject",
        Type = AttributeType.STRING,
      },
      SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
      {
        Name = "PartitionKey",
        Type = AttributeType.STRING,
      },
      ProjectionType = ProjectionType.ALL,
    });

    return table;
  }

  private RequestAuthorizer CreateAuthorizerFunction()
  {
    new StringParameter(this, "AuthorizerClientSecretParameter", new StringParameterProps
    {
      ParameterName = "/WDID/Authorizer/AuthorizationOptions/ClientSecret",
      StringValue = _configuration.GetSection("Authorizer").GetValue<string>("ClientSecret")!,
      Tier = ParameterTier.STANDARD,
    });

    var authorizerFunction = new AppFunction(this, "App.Authorizer", new AppFunction.Props(
      "App.Authorizer::App.Authorizer.Function::FunctionHandler"
    ));

    AllowSsm(authorizerFunction, "/WDID/Authorizer*", false);

    return new RequestAuthorizer(this, "ApiAuthorizer", new RequestAuthorizerProps
    {
      Handler = authorizerFunction,
      IdentitySources = new[] { IdentitySource.Header("authorization") },
      ResultsCacheTtl = Duration.Seconds(0),
    });
  }

  private void HandleLoginResource(
    Amazon.CDK.AWS.APIGateway.Resource loginResource)
  {
    var loginConfiguration = _configuration.GetSection("Login");
    new StringParameter(this, "LoginSigningCertificateParameter", new StringParameterProps
    {
      ParameterName = "/WDID/Login/SigningCertificate",
      StringValue = loginConfiguration.GetValue<string>("SigningCertificate")!,
      Tier = ParameterTier.STANDARD,
    });
    new StringParameter(this, "LoginEncryptionCertificateParameter", new StringParameterProps
    {
      ParameterName = "/WDID/Login/EncryptionCertificate",
      StringValue = loginConfiguration.GetValue<string>("EncryptionCertificate")!,
      Tier = ParameterTier.STANDARD,
    });

    var loginFunction = new AppFunction(this, "App.Login", new AppFunction.Props(
      "App.Login::App.Login.LambdaEntryPoint::FunctionHandlerAsync",
      _tableName,
      2048
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

    AllowSes(loginFunction);
    AllowSsm(loginFunction, "/WDID/DataProtection*", true);
    AllowSsm(loginFunction, "/WDID/Login*", false);

    loginResource.AddProxy(new ProxyResourceOptions
    {
      AnyMethod = true,
      DefaultIntegration = new LambdaIntegration(loginFunction),
    });
  }

  private void AllowSsm(AppFunction function, string resource, bool allowPut)
  {
    var actions = new List<string>
    {
      "ssm:GetParametersByPath",
    };

    if (allowPut)
    {
      actions.Add("ssm:PutParameter");
    }

    var ssmPolicy = new PolicyStatement(new PolicyStatementProps
    {
      Effect = Effect.ALLOW,
      Actions = actions.ToArray(),
      Resources = new[]
      {
        $"arn:aws:ssm:{this.Region}:{this.Account}:parameter{resource}",
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
    Table applicationTable,
    RequestAuthorizer authorizer)
  {
    // Create
    var createAccountFunction = new AppFunction(this, "CreateAccount", new AppFunction.Props(
      "CreateAccount::App.Api.CreateAccount.Function::FunctionHandler",
      _tableName
    ));
    applicationTable.GrantReadWriteData(createAccountFunction);
    accountResource.AddMethod("POST", new LambdaIntegration(createAccountFunction), new MethodOptions
    {
      AuthorizationType = AuthorizationType.CUSTOM,
      Authorizer = authorizer,
    });

    // List
    var listAccountsFunction = new AppFunction(this, "ListAccounts", new AppFunction.Props(
      "ListAccounts::App.Api.ListAccounts.Function::FunctionHandler",
      _tableName
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
    Table applicationTable,
    RequestAuthorizer authorizer)
  {
    // Create
    var createEventFunction = new AppFunction(this, "CreateEvent", new AppFunction.Props(
      "CreateEvent::App.Api.CreateEvent.Function::FunctionHandler",
      _tableName
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
      _tableName
    ));
    applicationTable.GrantReadWriteData(deleteEventFunction);
    eventResource.AddMethod("DELETE", new LambdaIntegration(deleteEventFunction), new MethodOptions
    {
      AuthorizationType = AuthorizationType.CUSTOM,
      Authorizer = authorizer,
    });

    // Edit
    var editEventFunction = new AppFunction(this, "EditEvent", new AppFunction.Props(
      "EditEvent::App.Api.EditEvent.Function::FunctionHandler",
      _tableName
    ));
    applicationTable.GrantReadWriteData(editEventFunction);
    eventResource.AddMethod("PUT", new LambdaIntegration(editEventFunction), new MethodOptions
    {
      AuthorizationType = AuthorizationType.CUSTOM,
      Authorizer = authorizer,
    });

    // List
    var listEventsFunction = new AppFunction(this, "ListEvents", new AppFunction.Props(
      "ListEvents::App.Api.ListEvents.Function::FunctionHandler",
      _tableName
    ));
    applicationTable.GrantReadData(listEventsFunction);
    eventResource.AddMethod("GET", new LambdaIntegration(listEventsFunction), new MethodOptions
    {
      AuthorizationType = AuthorizationType.CUSTOM,
      Authorizer = authorizer,
    });
  }

  private void HandleTagResource(
    Amazon.CDK.AWS.APIGateway.Resource tagResource,
    Table applicationTable,
    RequestAuthorizer authorizer)
  {
    // List
    var listTagsFunction = new AppFunction(this, "ListTags", new AppFunction.Props(
      "ListTags::App.Api.ListTags.Function::FunctionHandler",
      _tableName
    ));
    applicationTable.GrantReadData(listTagsFunction);
    tagResource.AddMethod("GET", new LambdaIntegration(listTagsFunction), new MethodOptions
    {
      AuthorizationType = AuthorizationType.CUSTOM,
      Authorizer = authorizer,
    });
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
      LogRetention = RetentionDays.ONE_DAY,
    });

    // Add X-Forwarded-Host Header
    var forwardedForFunction = new EdgeFunction(this, "ForwardedHostHeader", new EdgeFunctionProps
    {
      Code = Code.FromAsset("./src/App.Stack/ForwardedHostHeader"),
      Handler = "index.handler",
      Runtime = Runtime.NODEJS_18_X,
      LogRetention = RetentionDays.ONE_DAY,
    });

    // S3: Login
    var loginBucket = Bucket.FromBucketName(this, "Login", $"what-did-i-do-web-login");

    // S3: Account
    var accountBucket = Bucket.FromBucketName(this, "Account", $"what-did-i-do-web-account");

    // S3: Landing
    var landingBucket = Bucket.FromBucketName(this, "Landing", $"what-did-i-do-web-landing");

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

    // https://github.com/aws/aws-cdk/issues/21771#issuecomment-1281190832
    var oac = new CfnOriginAccessControl(this, "AOC", new CfnOriginAccessControlProps
    {
      OriginAccessControlConfig = new OriginAccessControlConfigProperty
      {
        Name = "AOC",
        OriginAccessControlOriginType = "s3",
        SigningBehavior = "always",
        SigningProtocol = "sigv4",
      },
    });

    var cfnDistribution = distribution.Node.DefaultChild as CfnDistribution;

    // Login
    cfnDistribution!.AddOverride("Properties.DistributionConfig.Origins.1.S3OriginConfig.OriginAccessIdentity", "");
    cfnDistribution!.AddPropertyOverride("DistributionConfig.Origins.1.OriginAccessControlId", oac.GetAtt("Id"));

    // Account
    cfnDistribution!.AddOverride("Properties.DistributionConfig.Origins.2.S3OriginConfig.OriginAccessIdentity", "");
    cfnDistribution!.AddPropertyOverride("DistributionConfig.Origins.2.OriginAccessControlId", oac.GetAtt("Id"));

    // Landing
    cfnDistribution!.AddOverride("Properties.DistributionConfig.Origins.3.S3OriginConfig.OriginAccessIdentity", "");
    cfnDistribution!.AddPropertyOverride("DistributionConfig.Origins.3.OriginAccessControlId", oac.GetAtt("Id"));

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
