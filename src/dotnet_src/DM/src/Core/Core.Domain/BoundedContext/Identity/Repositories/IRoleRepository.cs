using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.Repositories;

public interface IRoleRepository: IRepository
{
    Task<IdGuid> Create(Guid guidId, string name, string alias,  CancellationToken cancellationToken);
    
    
    Task<Role?> GetByAlias(string alias, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<Role>> GetListByAliases(string[] aliases, CancellationToken cancellationToken);

    Task<int> UpdateRoles(IdGuid id,
        string[] roleAliases,
        CancellationToken cancellationToken);
}