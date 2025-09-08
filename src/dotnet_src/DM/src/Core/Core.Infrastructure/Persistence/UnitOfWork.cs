using System.Data;
using Core.Application.Abstractions;
using Core.Application.EventHandlers;
using Core.Domain.SharedKernel.Events;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Core.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly EventHandlerProvider _eventHandlerProvider;
    private readonly IChangeTracker _changeTracker;
    private readonly IConnectionFactory<NpgsqlConnection> _connectionFactory;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(EventHandlerProvider eventHandlerProvider, IChangeTracker changeTracker,
        IConnectionFactory<NpgsqlConnection> connectionFactory, ILogger<UnitOfWork> logger)
    {
        _eventHandlerProvider = eventHandlerProvider;
        _changeTracker = changeTracker;
        _connectionFactory = connectionFactory;
        _logger = logger;
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

    public async ValueTask StartTransaction(CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        await _connectionFactory
            .InitialTransaction(cancellationToken, isolationLevel);
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

    public async ValueTask RollbackTransaction(CancellationToken cancellationToken)
    {
        await _connectionFactory.RollbackTransaction(cancellationToken);
    }

    public async ValueTask RollbackTransactionIfExist(CancellationToken cancellationToken)
    {
        if (_connectionFactory.CurrentTransaction != null)
        {
            await _connectionFactory.RollbackTransaction(cancellationToken);
        }
    }
}