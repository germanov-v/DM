using Core.Domain.BoundedContext.Identity.Aggregates;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Repositories;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.Repositories;

public interface ISessionRepository : IRepositoryGuidId<Session>
{ 
   // Task<IdGuid> Create(Session entity, CancellationToken cancellationToken);

    Task<int> RemoveById(long id, CancellationToken cancellationToken);

    Task<Session?> GetByRefreshTokenFingerprint(string refreshToken,
        string? fingerPrint, CancellationToken cancellationToken
    );


}