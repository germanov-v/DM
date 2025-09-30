using Core.Domain.SharedKernel.Entities.Abstractions;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.SharedKernel.Repositories;

public interface IRepository
{
    
}



public interface IRepositoryGuidId<T> : IRepository
     where T : class, IAggregateRoot
{
    Task<T?> GetById(long id, CancellationToken cancellationToken );
    Task<T?> GetByGuidId(Guid guidId, CancellationToken cancellationToken);
    Task<IdGuid> Create(T entity, CancellationToken cancellationToken );
    
    Task Update(T entity, CancellationToken cancellationToken );

    
    
}
   