using Core.Domain.SharedKernel.Entities.Abstractions;
using Core.Domain.SharedKernel.Entities.Abstractions.Types;

namespace Core.Domain.BoundedContext.Identity.Aggregates;

public sealed class Role : AggregateRoot, IGuidId
{
    public long Id { get; init; }
    public Guid GuidId { get; init; }
    public required string Name { get; init; }
    
    public required string Alias { get; init; }
}