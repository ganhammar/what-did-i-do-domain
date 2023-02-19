using TestBase;
using Xunit;

namespace CreateAccountTests;

[CollectionDefinition(Constants.DatabaseCollection)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
