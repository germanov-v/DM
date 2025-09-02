using Core.Domain.SharedKernel.Abstractions;

namespace Core.Application.Abstractions.Services;

public interface IDateTimeProvider : IApplicationService
{
   // DateTime UtcNow { get; }
    DateTimeOffset OffsetNow { get; }
}