using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.ValueObjects.Abstractions;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.ValueObjects;

public class IdGuid : ValueObject, IId
{
    public long ValueLong { get; }

    public Guid ValueGuid { get; private set; }

    public IdGuid(long valueLong, Guid valueGuid)
    {
        ValueLong = valueLong;
        ValueGuid = valueGuid;
    }
    
    private  IdGuid( Guid valueGuid)
    {
        ValueLong = 0;
        ValueGuid = valueGuid;
    }
    
    public static IdGuid New()=>new (Guid.NewGuid());
    
    public bool IsEmpty => ValueLong == 0;
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ValueLong;
        yield return ValueGuid;
    }

    public override string ToString()
        => $"{ValueLong}-{ValueGuid}";
}


[Obsolete]
class OldIdGuid : IId, IValueObject, IEquatable<OldIdGuid>
{
    private readonly long _longValue;
    
    private readonly Guid _guidValue;
    
    
    public OldIdGuid(long id, Guid guid)
    {
        _longValue = id;
        _guidValue = guid;
    }
    
    
    public long IdInt64 => _longValue;
    
    public Guid Guid => _guidValue;


    public bool Equals(OldIdGuid? other)
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
        return Equals((IdGuid)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_longValue, _guidValue);
    }
}