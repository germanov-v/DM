using Core.Domain.SharedKernel.Events;

namespace Core.Domain.SharedKernel.Entities;

public interface IEntity
{
    IReadOnlyCollection<IEvent>? Events { get; }

    void ClearDomainEvents();
}