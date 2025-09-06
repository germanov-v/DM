
using System.Collections.Immutable;
using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.Events;
using Microsoft.Extensions.Logging;

namespace Core.Application.EventHandlers;




public sealed class EventHandlerProvider
{
    
   

    private delegate ValueTask EventInvoker(IEntity entity, IEvent @evnet, CancellationToken cancellationToken);
    
    
    
 //   private readonly IServiceProvider _serviceProvider;
 
    // private readonly IServiceScopeFactory _serviceScopeFactory;
    //
    // public HandlerProvider(IServiceScopeFactory serviceScopeFactory)
    // {
    //     _serviceScopeFactory = serviceScopeFactory;
    // }
    
    private readonly ILogger<EventHandlerProvider> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly HandlerRegistry _handlerRegistry;
    public EventHandlerProvider(IServiceProvider serviceProvider, 
        HandlerRegistry handlerRegistry, 
        ILogger<EventHandlerProvider> logger)
    {
        _serviceProvider = serviceProvider;
        _handlerRegistry = handlerRegistry;
        _logger = logger;
    }

   
    
    public Type? GetInterfaceEvent(IEntity entity, IEvent @event, CancellationToken cancellationToken = default)
    {

        var key = new KeyEvent(entity.GetType(), @event.GetType());
        if (_handlerRegistry.Handlers.TryGetValue(key, out var type))
        {
            return type;
        }
        _logger.LogError($"No handler registered for interface {entity.GetType().Name} event {@event.GetType().Name}s");
        return null;
        //  throw new ArgumentException($"Invalid entity {entity.GetType().Name} event {@event.GetType().Name}");
    }
    
    
    public IEventHandler GetHandler(Type handlerInterface)
 //  public IEventHandler GetHandler(Type handlerInterface)

   {
        var handler = _serviceProvider.GetService(handlerInterface)
                      ?? throw new ArgumentException($"Invalid handler interface {handlerInterface.Name}");
       //   return (IEventHandler<IEntity, IEvent>)handler;
        return (IEventHandler)handler;

    }
    
}


public sealed class HandlerRegistry(IDictionary<KeyEvent, Type> handlers)
{
    public ImmutableDictionary<KeyEvent, Type> Handlers { get; } = handlers.ToImmutableDictionary();
}
