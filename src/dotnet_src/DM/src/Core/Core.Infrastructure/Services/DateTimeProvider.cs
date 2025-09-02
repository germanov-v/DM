using Core.Application.Abstractions.Services;

namespace Core.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; } = DateTime.UtcNow;
    public DateTimeOffset OffsetNow { get; } =  DateTimeOffset.Now;
}