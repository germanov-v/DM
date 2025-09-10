using System.Collections.Concurrent;
using Core.Domain.SharedKernel.ValueObjects.Abstractions;

namespace Core.Domain.SharedKernel.ValueObjects;

public sealed class AppDate : ValueObject<DateTimeOffset>
{
    public AppDate(DateTimeOffset value) : base(value)
    {
    }
    
    public static AppDate Create() => new AppDate(DateTimeOffset.Now);
     
   // public new DateTimeOffset Value => base.Value.UtcDateTime;
    
    public DateTime ValueByTimeZone(string timeZoneId) => base.Value.LocalDateTime;
}