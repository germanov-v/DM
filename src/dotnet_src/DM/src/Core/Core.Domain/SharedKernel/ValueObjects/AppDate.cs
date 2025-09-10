using System.Collections.Concurrent;
using Core.Domain.SharedKernel.ValueObjects.Abstractions;

namespace Core.Domain.SharedKernel.ValueObjects;

public sealed class AppDate : ValueObject<DateTimeOffset>
{
    public AppDate(DateTimeOffset value) : base(value)
    {
    }

    public DateTimeOffset InZone(string zoneId)
    {
        var tz = GetZone(zoneId);
        return TimeZoneInfo.ConvertTime(Value, tz); 
    }

    public AppDate UtcNow => new(DateTime.UtcNow);


    private static readonly ConcurrentDictionary<string, TimeZoneInfo> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    private static TimeZoneInfo GetZone(string id)
    {
        if (_cache.TryGetValue(id, out var tz)) return tz;
        tz = TimeZoneInfo.FindSystemTimeZoneById(id);


        return _cache[id] = tz;
    }
}