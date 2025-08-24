
using System.Collections.Immutable;
using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.Events;

namespace Core.Application.EventHandlers;




public sealed class HandlerProvider
{
    
   

    private delegate ValueTask EventInvoker(IEntity entity, IEvent @evnet, CancellationToken cancellationToken);
    
    
    
 //   private readonly IServiceProvider _serviceProvider;
 
    // private readonly IServiceScopeFactory _serviceScopeFactory;
    //
    // public HandlerProvider(IServiceScopeFactory serviceScopeFactory)
    // {
    //     _serviceScopeFactory = serviceScopeFactory;
    // }
    
    private readonly IServiceProvider _serviceProvider;
    private readonly HandlerRegistry _handlerRegistry;
    public HandlerProvider(IServiceProvider serviceProvider, HandlerRegistry handlerRegistry)
    {
        _serviceProvider = serviceProvider;
        _handlerRegistry = handlerRegistry;
    }

   
    
    public Type GetInterfaceEvent(IEntity entity, IEvent @event, CancellationToken cancellationToken = default)
    {

        var key = new KeyEvent(entity.GetType(), @event.GetType());
        if (_handlerRegistry.Handlers.TryGetValue(key, out var type))
        {
            return type;
        }

        throw new ArgumentException($"Invalid entity {entity.GetType().Name} event {@event.GetType().Name}");
    }
    
    
    public IEventHandler<IEntity, IEvent> GetHandler(Type handlerInterface)
    {
        var handler = _serviceProvider.GetService(handlerInterface)
                      ?? throw new ArgumentException($"Invalid handler interface {handlerInterface.Name}");
        return (IEventHandler<IEntity, IEvent>)handler;


    }
    
}


public sealed class HandlerRegistry(IDictionary<KeyEvent, Type> handlers)
{
    public ImmutableDictionary<KeyEvent, Type> Handlers { get; } = handlers.ToImmutableDictionary();
}
