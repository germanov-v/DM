using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.ValueObjects;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.Events;

public class ChangeTracker : IChangeTracker
{
    private ConcurrentQueue<IEntity> _entities = new();


    public void Track(IEntity entity) => _entities.Enqueue(entity);



    public IEnumerable<IEntity> GetAllAndClear()
    {
        var snapshot = Interlocked.Exchange(ref _entities, new ConcurrentQueue<IEntity>());
        while(snapshot.TryDequeue(out var entity))
            yield return entity;
    }

    public bool TryGetFirst(out IEntity? entity) => _entities.TryDequeue(out entity);
  
}

public class ChangeTrackerV4 //: IChangeTracker, IEnumerable<IEntity>
{
    private ConcurrentQueue<IEntity> _entities = new();


    public void Track(IEntity entity)
    {
        _entities.Enqueue(entity);
    }

    public IEntity? Pop()
    {
        if (_entities.TryDequeue(out IEntity? entity))
            return entity;


        return null;
    }

    // public IEnumerator<IEntity> GetEnumerator()
    // {
    //     if (_entities.TryDequeue(out IEntity? entity))
    //          yield return entity;
    // }
    //
    // IEnumerator IEnumerable.GetEnumerator()
    // {
    //     return GetEnumerator();
    // }
}

// public class ChangeTrackerV3 : IChangeTracker
// {
//     private ConcurrentBag<EntityRoot<IId>> _entities = new();
//
//     public IEnumerable<EntityRoot<IId>> TrackedEntities => _entities.AsEnumerable();
//
//     public void Track(EntityRoot<IId> entity)
//     {
//         var set = new HashSet<object>(ReferenceEqualityComparer.Instance);
//
//
//         //  ReferenceEqualityComparer.Instance
//         _entities.Add(entity);
//     }
// }

public class ChangeTrackerV2 : IChangeTrackerV2
{
    private ConcurrentBag<IEntity> _entities = new();

    public IEnumerable<IEntity> TrackedEntities => _entities.AsEnumerable();

    public void Track(IEntity entity)
    {
        _entities.Add(entity);
    }
}