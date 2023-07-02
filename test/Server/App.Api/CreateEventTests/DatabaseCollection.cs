using TestBase;

namespace CreateEventTests;

[CollectionDefinition(Constants.DatabaseCollection)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
