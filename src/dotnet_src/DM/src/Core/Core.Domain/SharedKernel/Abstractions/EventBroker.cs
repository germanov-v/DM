namespace Core.Domain.SharedKernel.Abstractions;

public class EventBroker
{
    private Dictionary<IEvent, List<ISubscriber>> Subscribers { get; }


    public async ValueTask<bool> PublishEvent(IEvent @event)
    {
        Subscribers.TryGetValue(@event, out var subscribers);

        if (subscribers != null)
        {
            foreach (var subscriber in subscribers)
            {
               await  subscriber.HandleEventAsync(@event);
            }
        }
        
    }
}