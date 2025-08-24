using Core.Application.Abstractions;
using Core.Application.EventHandlers;
using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.Events;

namespace Core.Application.SharedServices;

public class UnitOfWork : IUnitOfWork
{
    private readonly HandlerProvider _handlerProvider;
    private readonly IChangeTracker _changeTracker;

    public UnitOfWork(HandlerProvider handlerProvider, IChangeTracker changeTracker)
    {
        _handlerProvider = handlerProvider;
        _changeTracker = changeTracker;
    }

    public async ValueTask Commit(CancellationToken cancellationToken)
    {
        while (_changeTracker.TryGetFirst(out var entity))
        {
            if (entity?.Events is not null)
            {
                foreach (var entityEvent in entity.Events)
                {
                    var eventType = _handlerProvider.GetInterfaceEvent(entity, entityEvent);
                    var handler = _handlerProvider.GetHandler(eventType);
                    await handler.Handle(entity, entityEvent, cancellationToken);
                }
                entity.ClearDomainEvents();
            }
             
        }
    }
}