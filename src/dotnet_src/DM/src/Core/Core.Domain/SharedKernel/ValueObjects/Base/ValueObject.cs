using Core.Domain.SharedKernel.Abstractions;

namespace Core.Domain.SharedKernel.ValueObjects.Base;

public abstract class ValueObject : IValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj) =>
        GetType() == obj?.GetType()
        &&
        GetEqualityComponents().SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    
    
    public override int GetHashCode() =>
       GetEqualityComponents()
           .Select(x => x?.GetHashCode() ?? 0)  
           .Aggregate((x, y) => x ^ y);
    
    
    public ValueObject? GetCopy() => MemberwiseClone() as ValueObject;
    
    public static bool operator ==(ValueObject? left, ValueObject? right) =>
    // (left?.Equals(right)).GetValueOrDefault(); // TODO: BAD VARIANT null != null
    //  left is null == right is null && (left is null || left.Equals(right));
    Equals(left, right);
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}