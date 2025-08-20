using Core.Domain.SharedKernel.Abstractions;

namespace Core.Domain.SharedKernel.ValueObjects;

public class Status : IValueObject
{
    public bool Value { get; private set; }
    
    public DateTimeOffset ChangedAt { get; private set; }

    public Status(bool value, DateTimeOffset changedAt)
    {
        Value = value;
        ChangedAt = changedAt;
    }
}