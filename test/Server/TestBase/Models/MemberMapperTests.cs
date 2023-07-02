using App.Api.Shared.Models;

namespace TestBase.Models;

public class MemberMapperTests
{
  [Fact]
  public void Should_ReturnRole_When_SortKeyIsValid()
  {
    var member = new Member
    {
      PartitionKey = $"MEMBER#{Guid.NewGuid()}",
      SortKey = $"#ROLE#{Role.Owner.ToString()}#USER#{Guid.NewGuid()}"
    };

    Assert.Equal(Role.Owner, MemberMapper.GetRole(member));
  }
}
