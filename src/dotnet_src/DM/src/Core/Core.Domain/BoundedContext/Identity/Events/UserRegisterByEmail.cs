using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Events;

namespace Core.Domain.BoundedContext.Identity.Events;

public class UserRegisterByEmail : IEvent<User>
{
    public UserRegisterByEmail()
    {
     
    }

    public DateTimeOffset OccuredOn { get; } = DateTimeOffset.UtcNow;
}