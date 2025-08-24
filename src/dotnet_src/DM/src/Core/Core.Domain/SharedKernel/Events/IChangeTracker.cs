using Core.Domain.SharedKernel.Entities;

namespace Core.Domain.SharedKernel.Events;

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