using Core.Domain.BoundedContext.Identity.Aggregates;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.SharedKernel.ValueObjects;
using Core.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence.Repositories.Identity;

public class SessionRepository : ISessionRepository
{
     private readonly CoreDbContext _dbContext;

     public SessionRepository(CoreDbContext dbContext)
     {
         _dbContext = dbContext;
     }

     public async Task<IdGuid> Create(Session entity, CancellationToken cancellationToken)
    { 
        _dbContext.Sessions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new IdGuid(entity.Id, entity.GuidId);
    }
     
     
    // public Task<int> Remove(Session entity, CancellationToken cancellationToken)
    // {
    //     _dbContext.Sessions.Remove(entity);
    //     return _dbContext.SaveChangesAsync(cancellationToken);
    // }
  

    [Obsolete]
    public Task<int> Remove(long id, CancellationToken cancellationToken)
    {
        var entity = new Session
        {
            Id = id,
            Provider = null,
            AccessToken = null,
            RefreshToken = null,
            Fingerprint = null
        };
        
       // _dbContext.Sessions.Remove();
       throw new NotImplementedException();
    }

    public async Task<int> RemoveById(long id, CancellationToken cancellationToken)
    {
       return await _dbContext.Sessions
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<Session?> GetByRefreshTokenFingerprint(string refreshToken, string? fingerPrint,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Sessions
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.Fingerprint == fingerPrint,
                cancellationToken);
        
        
    }
    
    
    //////
    
    
    public Task<Session?> GetById(long id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Session?> GetByGuidId(Guid guidId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    public Task Update(Session entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}