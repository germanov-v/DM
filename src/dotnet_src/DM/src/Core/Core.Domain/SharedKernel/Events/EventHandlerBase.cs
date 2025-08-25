using Core.Domain.SharedKernel.Entities;

namespace Core.Domain.SharedKernel.Events;

public abstract class EventHandlerBase<TEntity, TEvent> 
     : IEventHandler<TEntity, TEvent>
where TEntity : IEntity
where TEvent : IEvent
{
     public abstract ValueTask Handle(TEntity entity, TEvent @event, CancellationToken cancellationToken);

     async ValueTask IEventHandler.Handle(IEntity entity, IEvent @event, CancellationToken cancellationToken)
    => await Handle((TEntity)entity, (TEvent)@event, cancellationToken);
}