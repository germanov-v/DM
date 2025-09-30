using Core.Domain.BoundedContext.Identity.Aggregates;
using Core.Domain.SharedKernel.Repositories;

namespace Core.Domain.BoundedContext.Identity.Repositories;

public interface IUserRepository : IRepositoryGuidId<User>
{
    
}