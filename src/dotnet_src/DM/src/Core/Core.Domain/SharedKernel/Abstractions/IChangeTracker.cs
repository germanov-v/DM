using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.ValueObjects;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.Abstractions;

public interface IChangeTracker
{
  //  IEnumerable<EntityRoot<IId>> TrackedEntities { get; }
    bool TryGetFirst(out IEntity? entity);
    
    void Track(IEntity entity);

    IEnumerable<IEntity> GetAllAndClear();
}

public interface IChangeTrackerV2
{
    IEnumerable<IEntity> TrackedEntities { get; }
    
    
    void Track(IEntity entity);
}