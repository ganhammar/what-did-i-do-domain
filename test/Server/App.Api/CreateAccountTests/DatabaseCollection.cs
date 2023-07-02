using TestBase;

namespace CreateAccountTests;

[CollectionDefinition(Constants.DatabaseCollection)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
