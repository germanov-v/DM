using Core.Domain.SharedKernel.ValueObjects.Abstractions;

namespace Core.Domain.SharedKernel.ValueObjects;

public class IdGuid : ValueObject
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