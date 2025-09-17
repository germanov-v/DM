using Core.Domain.SharedKernel.ValueObjects.Abstractions;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.SharedKernel.ValueObjects;

public class Status : ValueObject<bool> // IValueObject
{
    
    
    public DateTimeOffset ChangedAt { get; private set; }

    public Status(bool value, DateTimeOffset changedAt) : base(value)
    {
       ChangedAt = changedAt;
    }
}