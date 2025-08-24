using Core.Application.EventHandlers.Notifications;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Events;
using Core.Domain.SharedKernel.Events;
using Microsoft.
namespace Core.API.Extensions.DI;

public static class EventHandlersDiExtensions
{
    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {

        services.AddScoped<IEventHandler<User, UserRegisterByEmail>, UserRegisterByEmailHandler>();
        
        using var scope = services.BuildServiceProvider().CreateScope();
       var s1 = scope.ServiceProvider.GetRequiredService<IEventHandler<User, UserRegisterByEmail>>();

       var typeGeneric = typeof(IEventHandler<,>).MakeGenericType(typeof(User), typeof(UserRegisterByEmail));
       var s2 = (IEventHandler)scope.ServiceProvider.GetRequiredService(typeGeneric);
        return services;
    }
}