using Core.Domain.SharedKernel.Abstractions;

namespace Core.Domain.SharedKernel.ValueObjects;

public class Email : IValueObject// ValueObject<string>
{
    private readonly string _value;

    public Email(string value) 
    {
        _value = value;
    }
}