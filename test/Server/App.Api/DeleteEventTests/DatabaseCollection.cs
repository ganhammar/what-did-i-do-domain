using TestBase;

namespace DeleteEventTests;

[CollectionDefinition(Constants.DatabaseCollection)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
