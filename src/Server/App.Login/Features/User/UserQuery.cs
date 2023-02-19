using App.Login.Infrastructure;
using App.Login.Infrastructure.Validators;
using AspNetCore.Identity.AmazonDynamoDB;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace App.Login.Features.User;

public class UserQuery
{
  public class Query : IRequest<IResponse<UserDto>>
  {
  }

  public class QueryValidator : AbstractValidator<Query>
  {
    public QueryValidator(
      UserManager<DynamoDbUser> userManager,
      IHttpContextAccessor httpContextAccessor)
    {
      RuleFor(x => x)
        .MustAsync(async (query, cancellationToken) =>
        {
          if (httpContextAccessor.HttpContext?.User == default)
          {
            return false;
          }

          var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext?.User);

          return user != default;
        })
        .WithErrorCode(nameof(ErrorCodes.NoLoggedInUser))
        .WithMessage(ErrorCodes.NoLoggedInUser);
    }
  }

  public class QueryHandler : Handler<Query, IResponse<UserDto>>
  {
    private readonly UserManager<DynamoDbUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public QueryHandler(
      UserManager<DynamoDbUser> userManager,
      IHttpContextAccessor httpContextAccessor)
    {
      _userManager = userManager;
      _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IResponse<UserDto>> Handle(
      Query request, CancellationToken cancellationToken)
    {
      var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);

      return Response(UserMapper.ToDto(user));
    }
  }
}
