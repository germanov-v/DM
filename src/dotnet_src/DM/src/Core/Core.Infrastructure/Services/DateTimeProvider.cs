using Core.Application.Abstractions.Services;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
//    public DateTime UtcNow { get; } = DateTime.UtcNow;

    [Obsolete]
    public DateTimeOffset OffsetNow { get; } = AppDate.Now; // DateTimeOffset.Now;
}