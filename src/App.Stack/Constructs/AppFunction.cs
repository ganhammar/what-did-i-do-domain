using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Constructs;

namespace AppStack.Constructs;

public class AppFunction(Construct scope, string id, AppFunction.Props props)
  : Function(scope, $"{id}Function", new FunctionProps
  {
    Runtime = Runtime.DOTNET_6,
    Architecture = Architecture.ARM_64,
    Handler = props.Handler,
    Code = Code.FromAsset($"./{id}.zip"),
    Timeout = Duration.Minutes(1),
    MemorySize = props.MemorySize,
    LogRetention = RetentionDays.ONE_DAY,
    Environment = new Dictionary<string, string>
    {
      { "TABLE_NAME", props.TableName ?? "" },
    },
  })
{
  public class Props(string handler, string? tableName = default, int memorySize = 1024)
  {
    public string Handler { get; set; } = handler;
    public string? TableName { get; set; } = tableName;
    public int MemorySize { get; set; } = memorySize;
  }
}
