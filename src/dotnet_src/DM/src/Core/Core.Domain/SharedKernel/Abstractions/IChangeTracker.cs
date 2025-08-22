using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.ValueObjects;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.Abstractions;

public interface IChangeTracker
{
    IEnumerable<EntityRoot<IId>> TrackedEntities { get; }
    
    
    void Track(EntityRoot<IId> entity);
}

public interface IChangeTrackerV2
{
    IEnumerable<IEntity> TrackedEntities { get; }
    
    
    void Track(IEntity entity);
}