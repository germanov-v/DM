using Core.Domain.SharedKernel.Entities;

namespace Core.Domain.SharedKernel.Events;

public interface IEventHandler<in TEntity, in TEvent>
    where TEntity : IEntity
    where TEvent : IEvent<TEntity>
{
    ValueTask Handle(TEntity entity, TEvent @event, CancellationToken cancellationToken);
}

