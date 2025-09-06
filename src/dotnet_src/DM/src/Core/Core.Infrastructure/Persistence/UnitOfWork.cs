using System.Data;
using Core.Application.Abstractions;
using Core.Application.EventHandlers;
using Core.Domain.SharedKernel.Events;

namespace Core.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly EventHandlerProvider _eventHandlerProvider;
    private readonly IChangeTracker _changeTracker;

    public UnitOfWork(EventHandlerProvider eventHandlerProvider, IChangeTracker changeTracker)
    {
        _eventHandlerProvider = eventHandlerProvider;
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
                    var eventType = _eventHandlerProvider.GetInterfaceEvent(entity, entityEvent);
                    if (eventType is not null)
                    {
                        var handler = _eventHandlerProvider.GetHandler(eventType);
                        await handler.Handle(entity, entityEvent, cancellationToken);
                    }
              
                }
                entity.ClearDomainEvents();
            }
             
        }
    }

    public ValueTask StartTransaction(CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        throw new NotImplementedException();
    }

    public async ValueTask CommitTransaction(CancellationToken cancellationToken)
    {
        while (_changeTracker.TryGetFirst(out var entity))
        {
            if (entity?.Events is not null)
            {
                foreach (var entityEvent in entity.Events)
                {
                    var eventType = _eventHandlerProvider.GetInterfaceEvent(entity, entityEvent);
                    if (eventType is not null)
                    {
                        var handler = _eventHandlerProvider.GetHandler(eventType);
                        await handler.Handle(entity, entityEvent, cancellationToken);
                    }
              
                }
                entity.ClearDomainEvents();
            }
             
        }
    }

    public ValueTask RollbackTransaction(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ValueTask RollbackTransactionIfExist(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}