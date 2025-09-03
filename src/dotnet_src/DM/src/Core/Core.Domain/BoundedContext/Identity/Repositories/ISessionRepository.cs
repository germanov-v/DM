using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Abstractions;

namespace Core.Domain.BoundedContext.Identity.Repositories;

public interface ISessionRepository : IRepository
{ 
    Task<Session> Create(Session entity, 
        CancellationToken cancellationToken);

    Task<int> Remove(Session entity, CancellationToken cancellationToken);

    Task<Session?> GetByRefreshTokenFingerprint(string refreshToken,
        string fingerPrint,
        DateTimeOffset actualDate, CancellationToken cancellationToken
    );
}