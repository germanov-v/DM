using Core.Domain.SharedKernel.Abstractions;

namespace Core.Domain.SharedKernel.ValueObjects;

public class Id : IValueObject, IEquatable<Id>
{
    private readonly long _longValue;
    
    private readonly Guid _guidValue;
    
    
    public Id(long id, Guid guid)
    {
        _longValue = id;
        _guidValue = guid;
    }
    
    
    public long IdInt64 => _longValue;
    
    public Guid Guid => _guidValue;


    public bool Equals(Id? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _longValue == other._longValue && _guidValue.Equals(other._guidValue);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Id)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_longValue, _guidValue);
    }
}