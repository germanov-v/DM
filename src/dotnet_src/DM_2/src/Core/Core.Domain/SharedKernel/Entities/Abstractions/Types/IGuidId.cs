namespace Core.Domain.SharedKernel.Entities.Abstractions.Types;

public interface IGuidId
{
    public long Id { get; init; }
    
    public Guid GuidId { get; init; }
}