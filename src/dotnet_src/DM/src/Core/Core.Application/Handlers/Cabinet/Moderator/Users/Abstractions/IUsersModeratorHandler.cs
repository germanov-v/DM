using Core.Application.Abstractions.Services;
using Core.Application.Common.Results;
using Core.Application.Dto.Filter;
using Core.Application.Handlers.Cabinet.Moderator.Users.Dto;
using Core.Application.Handlers.Identity.Dto;

namespace Core.Application.Handlers.Cabinet.Moderator.Users.Abstractions;

public interface IUsersModeratorHandler : IApplicationService
{
    Task<Result<PaginatedResult<UserItem>>>  SearchUsers(PaginatedQuery<UserFilter> dto,
        CancellationToken cancellationToken);
}