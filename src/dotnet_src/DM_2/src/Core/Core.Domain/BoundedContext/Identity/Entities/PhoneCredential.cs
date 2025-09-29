using Core.Domain.SharedKernel.Entities.Abstractions;
using Core.Domain.SharedKernel.Entities.Abstractions.Types;

namespace Core.Domain.BoundedContext.Identity.Entities;

public class PhoneCredential: Entity, IConfirmed
{
    public required string Phone { get; init; }
    
    public bool IsConfirmed { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public string? ConfirmedCode { get; init; }
    public DateTimeOffset? ConfirmedCodeExpiresAt { get; init; }
}