using Core.Domain.SharedKernel.Events;

namespace Core.Domain.SharedKernel.Abstractions;

public interface ISubscriber
{
   ValueTask<bool> HandleEventAsync(IEvent @event);
}