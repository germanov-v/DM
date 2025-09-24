using Core.Application.Abstractions.Services;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Infrastructure.Services;

[Obsolete]
class DateTimeProvider : IDateTimeProvider
{
//    public DateTime UtcNow { get; } = DateTime.UtcNow;

    [Obsolete]
   public  DateTimeOffset OffsetNow { get; }// = AppDate; // DateTimeOffset.Now;
}