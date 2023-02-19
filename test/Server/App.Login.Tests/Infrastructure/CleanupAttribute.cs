using System.Reflection;
using Xunit.Sdk;

namespace App.Login.Tests.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class CleanupAttribute : BeforeAfterTestAttribute
{
  public override void After(MethodInfo methodUnderTest)
  {
    DynamoDbTestUtils.TruncateTable(DatabaseFixture.IdentityTableName, DatabaseFixture.Client)
      .GetAwaiter().GetResult();
    DynamoDbTestUtils.TruncateTable(DatabaseFixture.OpenIddictTableName, DatabaseFixture.Client)
      .GetAwaiter().GetResult();
  }
}
