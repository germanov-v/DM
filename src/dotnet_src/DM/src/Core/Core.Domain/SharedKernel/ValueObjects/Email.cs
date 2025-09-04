using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.ValueObjects.Abstractions;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.ValueObjects;

public class Email : StringValueObject
{
    public Email(string value) : base(value)
    {
    }
}