using Core.Domain.SharedKernel.Entities.Abstractions;
using Core.Domain.SharedKernel.Entities.Abstractions.Types;

namespace Core.Domain.BoundedContext.Identity.Entities;

public sealed class EmailCredential : Entity,IConfirmed
{
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }

    public bool IsConfirmed { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public string? ConfirmedCode { get; init; }
    public DateTimeOffset? ConfirmedCodeExpiresAt { get; init; }
}