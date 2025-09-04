using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.ValueObjects.Abstractions;

public abstract class StringValueObject : ValueObject<string>, IEquatable<StringValueObject>,  IComparable<StringValueObject>
{
    public StringValueObject(string value) : base(value)
    {
        if(String.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
    }

    public bool Equals(StringValueObject? other)
    {
        return other != null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StringValueObject)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public int CompareTo(StringValueObject? other)
    {
        return other is null? 1: String.Compare(Value, other.Value, StringComparison.Ordinal);
    }
}