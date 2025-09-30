using Core.Domain.BoundedContext.Identity.Aggregates;
using Core.Domain.SharedKernel.Repositories;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.Repositories;

public interface IRoleRepository : IRepositoryGuidId<Role>
{
    Task<Role?> GetByAlias(string alias, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<Role>> GetListByAliases(string[] aliases, CancellationToken cancellationToken);

    Task<int> UpdateRoles(IdGuid userId,
        string[] roleAliases,
        CancellationToken cancellationToken);
}