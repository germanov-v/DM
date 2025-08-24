using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using Core.Domain.BoundedContext.Identity.Events;
using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.Entities;

namespace Core.Domain.SharedKernel.Events;


[Obsolete("Проблема в том, что мы должны в Subscribe передать уже созданный экземпляр хэндлера")]
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
    
    public async ValueTask PublishEventV1(IEntity entity, IEvent @event, CancellationToken cancellationToken = default)
    {
        
        var key = new KeyEvent(entity.GetType(), @event.GetType());


        if (_handlers.TryGetValue(key, out var invokers))
        {
            foreach (var invoker in invokers)
                await invoker(entity, @event, cancellationToken);
        }
    }
    
    
   
    
  
}

public sealed class LocalEventBusReflection
{
    
    private readonly IServiceProvider _serviceProvider;

    public LocalEventBusReflection(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }


    public async ValueTask PublicAsync(IEntity entity, IEvent @event, CancellationToken cancellationToken)
    {
        var genericType = typeof(IEventHandler<,>).MakeGenericType(entity.GetType(), @event.GetType());
        
        var handlers = (IEnumerable<object>?) _serviceProvider.GetService(genericType);

        if (handlers != null)
        {
            foreach (var handler in handlers)
            {
                MethodInfo? mehod = genericType.GetMethod(nameof(IEventHandler<IEntity, IEvent<IEntity>>.Handle));
                if (mehod != null)
                {
                    var invoke = mehod.Invoke(handler, new object[]{entity.GetType(), @event, cancellationToken});
                    if (invoke != null)
                    {
                        await (ValueTask)invoke;
                    }
                }
 
            }     
        }
        
    }
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
