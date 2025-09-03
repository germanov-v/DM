using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Events;
using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.Events;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.UnitTests.Domain.Types;

public class RuntimeTypeHandlerTests
{

    [Fact]
    public void GetValue_ShouldNotThrow()
    {
        string test1 = "test";

        var handle = typeof(string).TypeHandle;
        

    }


    private class DataEventHandler : EventHandlerBase<User, UserRegisterByEmail>
    {
        public override ValueTask Handle(User entity, UserRegisterByEmail @event, CancellationToken cancellationToken)
        {
            
            return ValueTask.CompletedTask;
        }
    }
    
    [Fact]
    public void CastEventHandler_ShouldNotThrow()
    {
      
        object test1 = new DataEventHandler();
        
     //   var handler = (IEventHandler<User, UserRegisterByEmail>)test1;
        
     //   handler.Handle(new User(new IdGuid(1, Guid.Empty), ""), new UserRegisterByEmail(), CancellationToken.None);

        var handler2 = (IEventHandler)test1;
        //handler2.Handle(new User(new IdGuid(1, Guid.Empty), ""), null, CancellationToken.None);
    }
}