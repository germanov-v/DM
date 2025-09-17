using Core.Domain.SharedKernel.ValueObjects.Abstractions;

namespace Core.Domain.BoundedContext.Identity.ValueObjects;

public class Password : IValueObject, IEquatable<Password>
{
    public Password(string passwordHash, string passwordSalt)
    {
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }

    public string PasswordHash { get; private set; }
    public string PasswordSalt { get; private set; }
    
    public bool Equals(Password? other)
    {
        return other != null 
               && PasswordHash == other.PasswordHash 
               && PasswordSalt == other.PasswordSalt;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Password)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PasswordHash, PasswordSalt);
    }

    // public bool Equals(Password other)
    // {
    //     return PasswordHash == other.PasswordHash && PasswordSalt == other.PasswordSalt;
    // }
}