using Core.Domain.SharedKernel.Entities;

namespace Core.Domain.SharedKernel.Events;

public interface IEventHandler< TEntity, TEvent>
    : IEventHandler
    where TEntity : IEntity
    where TEvent : IEvent// IEvent<TEntity>
{
    ValueTask Handle(TEntity entity, TEvent @event, CancellationToken cancellationToken);
}

public interface IEventHandler
{
    ValueTask Handle(IEntity entity, IEvent @event, CancellationToken cancellationToken);
}



