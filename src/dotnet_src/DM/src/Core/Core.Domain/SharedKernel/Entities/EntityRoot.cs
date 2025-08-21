using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.Exceptions;

namespace Core.Domain.SharedKernel.Entities;

public abstract class EntityRoot<TKey> : IEntity
{
    private TKey _id = default!;

    public TKey Id
    {
        get => _id;
        set
        {
            if (!IsTransient)
               ThrowHelper.IdAlreadySet(GetType().Name, nameof(Id), typeof(TKey).Name);
                //    throw new InvalidOperationException($"{nameof(EntityRoot<TKey>)} property {nameof(Id)} can only be set once.");
                // throw new InvalidOperationException(
                //     $"{GetType().Name} property {nameof(Id)} with type {typeof(TKey).Name} can only be set once.");
             
            _id = value;
        }
    }


    public bool IsTransient => EqualityComparer<TKey>.Default.Equals(Id, default);
    //  typeof(TKey).IsValueType && EqualityComparer<TKey>.Default.Equals(_id, default(TKey));
    //  ReferenceEquals(_id, default(TKey))||_id.Equals(default(TKey))
}