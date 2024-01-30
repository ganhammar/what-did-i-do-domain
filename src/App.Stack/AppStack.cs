using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Experimental;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.S3;
using Constructs;
using static Amazon.CDK.AWS.CloudFront.CfnDistribution;
using static Amazon.CDK.AWS.CloudFront.CfnOriginAccessControl;

namespace AppStack;

public class AppStack : Stack
{
  private const string TableName = "what-did-i-do";

  internal AppStack(Construct scope, string id, IStackProps props)
    : base(scope, id, props)
  {
    // DynamoDB
    CreateTable();

    // EventBridge Bus
    CreateEventBus();

    // CloudFront Distribution
    var cloudFrontDistribution = CreateCloudFrontWebDistribution();

    // Output
    _ = new CfnOutput(this, "CloudFrontDomainName", new CfnOutputProps
    {
      Value = cloudFrontDistribution.DistributionDomainName,
    });
  }

  private Table CreateTable()
  {
    var table = new Table(this, "ApplicationTable", new TableProps
    {
      TableName = TableName,
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
      Stream = StreamViewType.NEW_AND_OLD_IMAGES,
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

    _ = new CfnOutput(this, "ApplicationTableStreamArn", new CfnOutputProps
    {
      Value = table.TableStreamArn!,
      ExportName = $"{Of(this).StackName}-ApplicationTableStreamArn",
    });

    return table;
  }

  private EventBus CreateEventBus()
  {
    var eventBus = new EventBus(this, "DomainBus", new EventBusProps
    {
      EventBusName = "DomainBus",
    });

    // Grant permissions to all functions in the account to put events to the EventBridge bus
    var accountRootPrincipal = new AccountRootPrincipal();
    var policyStatement = new PolicyStatement(new PolicyStatementProps
    {
      Sid = "AllowPutEventsForAccount",
      Actions = ["events:PutEvents"],
      Resources = [eventBus.EventBusArn],
      Principals = [accountRootPrincipal],
    });
    eventBus.AddToResourcePolicy(policyStatement);

    return eventBus;
  }

  private CloudFrontWebDistribution CreateCloudFrontWebDistribution()
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
    var loginBucket = Bucket.FromBucketName(this, "Login", "what-did-i-do-web-login");

    // S3: Account
    var accountBucket = Bucket.FromBucketName(this, "Account", "what-did-i-do-web-account");

    // S3: Landing
    var landingBucket = Bucket.FromBucketName(this, "Landing", "what-did-i-do-web-landing");

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
              DomainName = $"ukfgt58q78.execute-api.{Region}.{UrlSuffix}",
              OriginPath = "/prod",
            },
            Behaviors = new[]
            {
              new Behavior
              {
                PathPattern = "/api/login/*",
                AllowedMethods = CloudFrontAllowedMethods.ALL,
                DefaultTtl = Duration.Seconds(0),
                ForwardedValues = new ForwardedValuesProperty
                {
                  QueryString = true,
                  Headers = ["Authorization"],
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
            CustomOriginSource = new CustomOriginConfig
            {
              DomainName = $"a45c9a7715.execute-api.{Region}.{UrlSuffix}",
              OriginPath = "/prod",
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
                  Headers = ["Authorization"],
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
          Aliases =
          [
            "wdid.fyi",
            "www.wdid.fyi",
          ],
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
    cfnDistribution!.AddOverride("Properties.DistributionConfig.Origins.2.S3OriginConfig.OriginAccessIdentity", "");
    cfnDistribution!.AddPropertyOverride("DistributionConfig.Origins.2.OriginAccessControlId", oac.GetAtt("Id"));

    // Account
    cfnDistribution!.AddOverride("Properties.DistributionConfig.Origins.3.S3OriginConfig.OriginAccessIdentity", "");
    cfnDistribution!.AddPropertyOverride("DistributionConfig.Origins.3.OriginAccessControlId", oac.GetAtt("Id"));

    // Landing
    cfnDistribution!.AddOverride("Properties.DistributionConfig.Origins.4.S3OriginConfig.OriginAccessIdentity", "");
    cfnDistribution!.AddPropertyOverride("DistributionConfig.Origins.4.OriginAccessControlId", oac.GetAtt("Id"));

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

    _ = new AaaaRecord(this, "AliasRecord", new AaaaRecordProps
    {
      Target = RecordTarget.FromAlias(new CloudFrontTarget(distribution)),
      Zone = zone,
    });

    _ = new CnameRecord(this, "CnameRecord", new CnameRecordProps
    {
      RecordName = "www.wdid.fyi",
      DomainName = distribution.DistributionDomainName,
      Ttl = Duration.Minutes(5),
      Zone = zone,
    });
  }
}
