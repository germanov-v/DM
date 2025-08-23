using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Events;

namespace Core.Domain.BoundedContext.Identity.Events;

public class UserRegisterByEmail : IEvent<User>
{
    public UserRegisterByEmail(DateTimeOffset occuredOn)
    {
        OccuredOn = occuredOn;
    }

    public DateTimeOffset OccuredOn { get; }
}