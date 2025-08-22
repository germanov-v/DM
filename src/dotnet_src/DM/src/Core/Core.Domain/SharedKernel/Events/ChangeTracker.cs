using System.Collections.Concurrent;
using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.ValueObjects;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.Events;

public class ChangeTracker : IChangeTracker
{

    private ConcurrentBag<EntityRoot<IId>> _entities = new();
    
    public IEnumerable<EntityRoot<IId>> TrackedEntities  => _entities.AsEnumerable(); 
    
    public void Track(EntityRoot<IId> entity)
    {
        _entities.Add(entity);
    }
}

public class ChangeTrackerV2 : IChangeTrackerV2
{

    private ConcurrentBag<IEntity> _entities = new();
    
    public IEnumerable<IEntity> TrackedEntities  => _entities.AsEnumerable(); 
    
    public void Track(IEntity entity)
    {
        _entities.Add(entity);
    }
}