using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace AppStack.Constructs;

public class AppFunction : Function
{
  public AppFunction(Construct scope, string id, Props props)
    : base(scope, $"{id}Function", new FunctionProps
    {
      Runtime = Runtime.DOTNET_6,
      Architecture = Architecture.ARM_64,
      Handler = props.Handler,
      Code = Code.FromAsset($"./.output/{id}.zip"),
      Timeout = Duration.Minutes(1),
      MemorySize = 128,
      Environment = new Dictionary<string, string>
    {
      { "TABLE_NAME", props.TableName },
    }
    })
  { }

  public class Props
  {
    public Props(string handler, string tableName)
    {
      Handler = handler;
      TableName = tableName;
    }

    public string Handler { get; set; }
    public string TableName { get; set; }
  }
}
