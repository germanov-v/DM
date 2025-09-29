using System.Net;
using Core.Domain.SharedKernel.Entities.Abstractions;
using Core.Domain.SharedKernel.Entities.Abstractions.Types;

namespace Core.Domain.BoundedContext.Identity.Aggregates;

public sealed class Session : AggregateRoot, IGuidId
{
    public long Id { get; init; }
    public Guid GuidId { get; init; }
    
    public long UserId { get; init; }
    public required string Provider { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; init; }
    public required string Fingerprint { get; init; }
    public IPAddress? IpAddress { get; init; }
}