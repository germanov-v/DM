namespace Core.Domain.SharedKernel.Entities.Abstractions.Types;

public interface IConfirmed
{
    
    public bool IsConfirmed { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public string? ConfirmedCode { get; init; }
    public DateTimeOffset? ConfirmedCodeExpiresAt { get; init; }

}