using Core.Domain.SharedKernel.Entities.Abstractions;
using Core.Domain.SharedKernel.Entities.Abstractions.Types;

namespace Core.Domain.BoundedContext.Identity.Entities;

public class ExternalIdentity : Entity, IGuidId
{
    public long Id { get; init; }
    public Guid GuidId { get; init; }
    
    public required string Provider { get; init; }
    public required string ProviderUserId { get; init; }
    public string? EmailFromProvider { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTimeOffset? TokenExpiresAt { get; init; }
    
    public string? RawProfileJson { get; init; }
    public bool IsPrimary { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? LastUsedAt { get; init; }
}