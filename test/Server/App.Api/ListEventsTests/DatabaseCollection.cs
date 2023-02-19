using TestBase;
using Xunit;

namespace ListEventsTests;

[CollectionDefinition(Constants.DatabaseCollection)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
