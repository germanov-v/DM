namespace Core.Domain.SharedKernel.Abstractions;

public  abstract class EntityRoot<TKey> :  IEntity
{
    private TKey _id = default!;

    public TKey Id
    {
        get => _id;
        set
        {
            if (!IsTransient)
                throw new InvalidOperationException($"{nameof(EntityRoot<TKey>)} property {nameof(Id)} can only be set once.");
            _id = value;
        }
    }
    
    
    public bool IsTransient => EqualityComparer<TKey>.Default.Equals(Id, default);
      //  typeof(TKey).IsValueType && EqualityComparer<TKey>.Default.Equals(_id, default(TKey));
      //  ReferenceEquals(_id, default(TKey))||_id.Equals(default(TKey))
        
       
}