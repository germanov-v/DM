using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Abstractions;

namespace Core.Domain.BoundedContext.Identity.Repositories;

public interface IRoleRepository: IRepository
{
    Task<Role> Create(Role entity, CancellationToken cancellationToken);
    
    
    Task<Role?> GetByAlias(string alias, CancellationToken cancellationToken);
    
    Task<List<Role>> GetListByAliases(string[] aliases, CancellationToken cancellationToken);
}