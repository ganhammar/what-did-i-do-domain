using AspNetCore.Identity.AmazonDynamoDB;

namespace App.Login.Features.User;

public static class UserMapper
{
  public static UserDto ToDto(DynamoDbUser user)
    => new UserDto
    {
      Id = user.Id,
      Email = user.Email,
      PhoneNumber = user.PhoneNumber,
    };
}
