using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.ValueObjects;


public class Id : ValueObject<long>, IId
{
    public Id(long value) : base(value)
    {
    }
}


[Obsolete]
class DraftIdCustom: IValueObject, IEquatable<Id>,IId
{

    private long _id;
    
    
    
    public bool Equals(Id? other)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}