using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.ValueObjects;

public class Phone : ClassValueObject<string>
{
    public Phone(string value) : base(value)
    {
    }
}