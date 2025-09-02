using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;

namespace Core.Infrastructure.Persistence.Repositories.Identity;

public class SessionRepository : ISessionRepository
{
    public Task<Session> Create(Session entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> Remove(Session entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Session?> GetByRefreshTokenFingerprint(string refreshToken, string fingerPrint, DateTimeOffset actualDate,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}