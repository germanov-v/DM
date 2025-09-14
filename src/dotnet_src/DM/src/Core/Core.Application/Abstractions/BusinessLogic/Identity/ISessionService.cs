using Core.Application.Abstractions.Services;
using Core.Application.Common.Results;
using Core.Domain.BoundedContext.Identity.Entities;

namespace Core.Application.Abstractions.BusinessLogic.Identity;

public interface ISessionService : IApplicationService
{
    Task<Result> Create(Session session, CancellationToken cancellationToken);
    
    Task<Result<Session>> GetValidSession(string refreshToken, string? fingerPrint,
        DateTimeOffset date,
        CancellationToken cancellationToken);

    Task<Result<(User User, Session Session)>> GetUserBySession(string refreshToken, string? fingerPrint, DateTimeOffset date,
        CancellationToken cancellationToken);

    Task<Result> RemoveSessionById(long id, CancellationToken cancellationToken);
}