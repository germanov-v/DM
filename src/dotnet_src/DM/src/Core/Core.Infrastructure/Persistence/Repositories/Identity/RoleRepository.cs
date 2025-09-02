using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;

namespace Core.Infrastructure.Persistence.Repositories.Identity;

public class RoleRepository : IRoleRepository
{
    public Task<Role> Create(Role entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Role?> GetByAlias(string alias, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<Role>> GetListByAliases(string[] aliases, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}