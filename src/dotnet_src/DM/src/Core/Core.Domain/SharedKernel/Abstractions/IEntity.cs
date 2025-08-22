namespace Core.Domain.SharedKernel.Abstractions;

public interface IEntity
{
    IReadOnlyCollection<IEvent>? Events { get; }
}