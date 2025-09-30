namespace Core.Domain.SharedKernel.ValueObjects.Abstractions;

public abstract class ValueObject : IValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType())
            return false;
        
        var other = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());

        // return   GetType() == obj?.GetType()
        //          &&
        //          GetEqualityComponents().SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }
      
    
    
    // public override int GetHashCode() =>
    //    GetEqualityComponents()
    //        .Select(x => x?.GetHashCode() ?? 0)  
    //        .Aggregate((x, y) => x ^ y);


    public override int GetHashCode()
    {
        var hash = new HashCode();
        
        foreach (var component in GetEqualityComponents())
            hash.Add(component);
        return hash.ToHashCode();
    }
    
    
    public ValueObject GetCopy() => (ValueObject)MemberwiseClone();
    
    public static bool operator ==(ValueObject? left, ValueObject? right) =>
    // (left?.Equals(right)).GetValueOrDefault(); // TODO: BAD VARIANT null != null
    //  left is null == right is null && (left is null || left.Equals(right));
    Equals(left, right);
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}

public abstract class ValueObject<TBaseType> : ValueObject where TBaseType : IComparable<TBaseType>
{
    public TBaseType Value { get; }
    
    public ValueObject(TBaseType value) => Value = value;


    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string? ToString() => Value.ToString();
}


public abstract class ClassValueObject<TBaseType> : ValueObject<TBaseType> where TBaseType : class, IComparable<TBaseType>
{
    protected ClassValueObject(TBaseType value) : base(value)
    {
    }
    
    public static implicit operator TBaseType?(ClassValueObject<TBaseType>? value) => value?.Value;
    
    
  //  public static explicit operator TBaseType(ClassValueObject<TBaseType> value) => value.Value;
}