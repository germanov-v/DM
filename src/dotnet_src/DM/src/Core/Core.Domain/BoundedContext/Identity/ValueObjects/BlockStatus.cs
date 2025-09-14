using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.ValueObjects;

public class BlockStatus : Status
{
    
    public int? Code { get; }
    
    public string? Reason { get; }
    
    public BlockStatus(bool value, DateTimeOffset changedAt, int? code, string? reason) : base(value, changedAt)
    {
        Code = code;
        Reason = reason;
    }
}