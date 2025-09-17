using System.Runtime.CompilerServices;
using Core.Domain.SharedKernel.Events;
using Core.Domain.SharedKernel.Exceptions;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.Entities;

public abstract class EntityRoot<TKey> : IEntity 
{
    private TKey _id = default!;
    
    private int? _requestedHashCode;

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
    
    private List<IEvent>? _events = new ();
    
    public IReadOnlyCollection<IEvent>? Events => _events?.AsReadOnly();

    public void AddDomainEvent(IEvent @event)
    {
        _events ??= new List<IEvent>();
        _events.Add(@event);
    }

    public void ClearDomainEvents()
    {
        _events?.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not EntityRoot<TKey> other) return false;
        
        if(ReferenceEquals(this, other)) return true;
        
        if(GetType() != other.GetType()) return false;
        
        if(IsTransient || other.IsTransient) return false;
        
        if (Id is null) return false;
        
        return Id.Equals(other.Id);
    }

    // protected bool Equals(EntityRoot<TKey> other)
    // {
    //     return EqualityComparer<TKey>.Default.Equals(_id, other._id) && Equals(_events, other._events);
    // }
    public override int GetHashCode()
    {
       // return HashCode.Combine(_id, _events);
       //if (IsTransient) return base.GetHashCode();

       // явно хэш по ссылке
       if (IsTransient)
           return RuntimeHelpers.GetHashCode(this);
       
       if(Id==null) 
           throw new InvalidOperationException("NonTransient entity must have an Id.");
       
       // ReSharper disable once NonReadonlyMemberInGetHashCode
       _requestedHashCode ??= Id.GetHashCode(); 
       
       // ReSharper disable once NonReadonlyMemberInGetHashCode
       return _requestedHashCode.Value;
    }
    
    public static bool operator ==(EntityRoot<TKey>? left, EntityRoot<TKey>? right)=>
        left is null == right is null && (left is null || left.Equals(right));

    public static bool operator !=(EntityRoot<TKey>? left, EntityRoot<TKey>? right) => !(left == right);
}