using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Entities.Abstractions;
using Core.Domain.SharedKernel.Entities.Abstractions.Types;

namespace Core.Domain.BoundedContext.Identity.Aggregates;

public sealed class User : AggregateRoot, IGuidId
{

    public long Id { get; init; }
    public Guid GuidId { get; init; }
    
    
    
    public required string Name { get; init; }
    
    public bool IsConfirmed { get; init; }

    public DateTimeOffset ConfirmedChangeAt { get; init; }
    
    public bool IsBlocked { get; init; }
    
    public DateTimeOffset BlockedChangedAt { get; init; }
    
    public int? BlockedReasonCode { get; init; }
    public string? BlockedReason { get; init; }
    
    
    public EmailCredential Email { get; init; }
    public PhoneCredential Phone { get; init; }
    
    private readonly List<ExternalIdentity> _externalIdentities = new();
    public IEnumerable<ExternalIdentity> ExternalIdentities => _externalIdentities;
    
    private readonly HashSet<long> _roleIds = new();
    public IReadOnlyCollection<long> RoleIds => _roleIds;

}