using Core.Domain.SharedKernel.Entities.Abstractions;

namespace Core.Domain.SharedKernel.Repositories;

public interface IRepository
{
    
}



public interface IRepository<T> : IRepository
     where T : class, IAggregateRoot
{
    Task<T?> GetById(long id, CancellationToken cancellationToken );
    Task<T?> GetByGuidId(Guid guidId, CancellationToken cancellationToken);
    Task Add(T entity, CancellationToken cancellationToken );
    
}
   