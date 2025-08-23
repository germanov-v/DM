using Core.Domain.SharedKernel.Entities;

namespace Core.Domain.SharedKernel.Events;

public interface IEvent
{
    DateTimeOffset OccuredOn { get; }
}


public interface IEvent<TEntity> : IEvent where TEntity : IEntity
{
    
}


