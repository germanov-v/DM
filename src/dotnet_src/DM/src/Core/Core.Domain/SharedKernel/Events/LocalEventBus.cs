using System.Collections.Concurrent;
using System.Collections.Immutable;
using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.Entities;

namespace Core.Domain.SharedKernel.Events;

public sealed class LocalEventBus
{

    private delegate ValueTask EventInvoker(IEntity entity, IEvent @evnet, CancellationToken cancellationToken);
    
    
    
    private readonly ConcurrentDictionary<KeyEvent, ImmutableArray<EventInvoker>> _handlers = new();


    public void Subscribe<TEntity, TEvent>(IEventHandler<TEntity, TEvent> handler)
        where TEntity : class, IEntity
        where TEvent : class, IEvent<TEntity>
    {
        
        // TODO:  захват внешней переменной - аллокация
        EventInvoker invoker =   async (entity, @event, cancellationToken) =>
        {
            await handler.Handle((TEntity)entity, (TEvent)@event, cancellationToken);
        };
        
        var key = new KeyEvent(typeof(TEntity), typeof(TEvent));
        
        _handlers.AddOrUpdate(key,
            _=> ImmutableArray.Create<EventInvoker>(invoker),
            (_, arr) => arr.Add(invoker)
            );
    }
    
    public async ValueTask PublishEvent(IEntity entity, IEvent @event, CancellationToken cancellationToken = default)
    {
        
        var key = new KeyEvent(entity.GetType(), @event.GetType());


        if (_handlers.TryGetValue(key, out var invokers))
        {
            foreach (var invoker in invokers)
                await invoker(entity, @event, cancellationToken);
        }
    }
    
    
    // private delegate ValueTask EventInvokerStatic(object state, IEntity aggregate, IEvent @event, CancellationToken ct);
    // private readonly record struct Entry(EventInvokerStatic Invoker, object State);
    // public void SubscribeV2<TEntity, TEvent>(IEventHandler<TEntity, TEvent> handler)
    //     where TEntity : class, IEntity
    //     where TEvent   : class, IEvent<TEntity>
    // {
    //     var key = new KeyEvent(typeof(TEntity), typeof(TEvent));
    //
    //     var entry = new Entry(
    //         static (state, agg, ev, ct) =>
    //             ((IEventHandler<TEntity, TEvent>)state).Handle((TEntity)agg, (TEvent)ev, ct),
    //         handler);
    //
    //    
    // }
    
    
  
}


public readonly struct KeyEvent : IEquatable<KeyEvent>
{

    public readonly RuntimeTypeHandle Entity;
    public readonly RuntimeTypeHandle Event;

    public KeyEvent(Type entity, Type @event)
    {
        Entity = entity.TypeHandle;
        Event = @event.TypeHandle;
    }


    public bool Equals(KeyEvent other)
    {
        return Entity.Equals(other.Entity) && Event.Equals(other.Event);
    }

    public override bool Equals(object? obj)
    {
        return obj is KeyEvent other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Entity, Event);
    }
}