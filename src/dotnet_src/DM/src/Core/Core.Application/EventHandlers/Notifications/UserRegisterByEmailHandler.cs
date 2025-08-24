using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Events;
using Core.Domain.SharedKernel.Events;

namespace Core.Application.EventHandlers.Notifications;

public class UserRegisterByEmailHandler : IEventHandler<User, UserRegisterByEmail>
{
    
    public ValueTask Handle(User entity, UserRegisterByEmail @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}