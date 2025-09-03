using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.ValueObjects;

public class Name : ValueObject<string>
{
    public Name(string value) : base(value)
    {
    }
}