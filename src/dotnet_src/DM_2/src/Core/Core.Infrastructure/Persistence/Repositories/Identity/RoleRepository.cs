using Core.Domain.BoundedContext.Identity.Aggregates;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.SharedKernel.ValueObjects;
using Core.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence.Repositories.Identity;

public class RoleRepository : IRoleRepository
{
  
    private readonly CoreDbContext _context;

    public RoleRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<IdGuid> Create(Role entity, CancellationToken cancellationToken)
    {
        _context.Roles.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return new IdGuid(entity.Id, entity.GuidId);
    }

 

    public async Task<Role?> GetByAlias(string alias, CancellationToken cancellationToken)
    =>await _context.Roles.FirstOrDefaultAsync(p=>p.Alias == alias, cancellationToken);

    public async Task<IReadOnlyList<Role>> GetListByAliases(string[] aliases, CancellationToken cancellationToken)
    {
        var roles = await _context.Roles
            .Where(p=>aliases.Contains(p.Alias))
            
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return roles;
    }

    public async Task<int> UpdateRoles(IdGuid userId, string[] roleAliases, CancellationToken cancellationToken)
    { 
        var currentTx = _context.Database.CurrentTransaction;

        if (currentTx != null)
        {
            throw new InvalidOperationException("Transaction was not started");
        }
        
        // await _context
        //     .Rol;

        return 0;
    }
    
    ////////
    
    public Task<Role?> GetById(long id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Role?> GetByGuidId(Guid guidId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    public Task Update(Role entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}