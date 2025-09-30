using Core.Domain.BoundedContext.Identity.Aggregates;
using Core.Domain.SharedKernel.Repositories;
using Core.Domain.SharedKernel.ValueObjects;
using Core.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence.Repositories.Identity;

public class UserRepository : IRepositoryGuidId<User>
{
    private readonly CoreDbContext  _dbContext;

    public UserRepository(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetById(long id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
     }

    public async Task<User?> GetByGuidId(Guid guidId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(p => p.GuidId == guidId, cancellationToken);
    }

    public async Task<IdGuid> Create(User entity, CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new IdGuid(entity.Id, entity.GuidId);
    }

    public async Task Update(User entity, CancellationToken cancellationToken)
    {
        _dbContext.Users.Update(entity);
         await _dbContext.SaveChangesAsync(cancellationToken);
    }
}