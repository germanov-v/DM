using Core.Application.Common.Results;
using Core.Application.Dto.Filter;
using Core.Application.Handlers.Cabinet.Moderator.Users.Abstractions;
using Core.Application.Handlers.Cabinet.Moderator.Users.Dto;

namespace Core.Application.Handlers.Cabinet.Moderator.Users;

public class UsersModeratorHandler : IUsersModeratorHandler
{
    public Task<Result<PaginatedResult<UserItem>>> SearchUsers(PaginatedQuery<UserFilter> dto, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}