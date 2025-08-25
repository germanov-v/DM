using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Events;
using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.Events;
using Microsoft.Extensions.Logging;

namespace Core.Application.EventHandlers.Notifications;


public class UserRegisterByEmailHandler :  EventHandlerBase<User, UserRegisterByEmail>
{
    private readonly ILogger<UserRegisterByEmailHandler> _logger;

    public UserRegisterByEmailHandler(ILogger<UserRegisterByEmailHandler> logger)
    {
        _logger = logger;
    }

    public override ValueTask Handle(User entity, UserRegisterByEmail @event, CancellationToken cancellationToken)
    {
       _logger.LogInformation("----- Handling User {UserEmail} ----", entity.Email);
       
       return ValueTask.CompletedTask;
    }

    public ValueTask Handle(IEntity entity, IEvent @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}