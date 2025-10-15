namespace Core.Application.Abstractions.Services;


[Obsolete]
public interface IDateTimeProvider // : IApplicationService
{
   // DateTime UtcNow { get; }
    DateTimeOffset OffsetNow { get; }
}