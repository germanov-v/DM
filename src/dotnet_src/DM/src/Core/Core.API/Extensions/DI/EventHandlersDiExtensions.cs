using System.Reflection;
using Core.Application.EventHandlers;
using Core.Application.EventHandlers.Notifications;
using Core.Application.Services.Identity;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Events;
using Core.Domain.SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core.API.Extensions.DI;

public static class EventHandlersDiExtensions
{
    public static IServiceCollection AddEventHandlers(this IServiceCollection services,
          params Assembly[] assemblies
        )
    {
        // var assemblies = new Assembly[] {
        //     typeof(EventBus).Assembly,
        // };

        
        
        
        var handlerInterfaceType = typeof(IEventHandler<,>);
        // var classes =
        //     assemblies
        //         .SelectMany(p =>
        //             p.GetTypes().Where(type =>
        //                 type is
        //                 {
        //                     IsClass: true,
        //                     IsAbstract: false
        //                 } &&
        //                 handlerInterfaceType.IsAssignableFrom(type)
        //                 
        //             ));
        
        var classes = assemblies.SelectMany(a=>a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false });

        var dictionary = new Dictionary<KeyEvent, Type>();
        
        foreach (var type in classes)
        {
            if(type.IsAbstract||type.IsInterface) continue;
            
            // open generics
            if (type.IsGenericTypeDefinition
                &&
                type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                )
            {
                services.TryAddEnumerable(ServiceDescriptor.Transient(handlerInterfaceType, type));
                continue;
            }
            
            // closed generic
            var closed = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType);

            foreach (var closeInterface in closed)
            {
                services.TryAddEnumerable(ServiceDescriptor.Transient(closeInterface, type));
                
                var key = GetKeyEvent(closeInterface);
                dictionary.Add(key, closeInterface);
            //    handlerProvider.Subscribe(key, closeInterface);
            }
            
        }
        var handlerRegistry = new HandlerRegistry(dictionary);
        services.AddSingleton(handlerRegistry);
        services.AddScoped<HandlerProvider>();
     
        return services;
    }


    public static KeyEvent GetKeyEvent(Type type)
    {
        if (type.IsGenericType)
        {
            var entity = type.GetGenericArguments()[0];
            var @event = type.GetGenericArguments()[1];
            return new KeyEvent(entity, @event);
        }
        throw new InvalidOperationException($"Type {type} is not a generic type");
    }
    
    //  services.AddScoped<IEventHandler<User, UserRegisterByEmail>, UserRegisterByEmailHandler>();
    //  
    //  using var scope = services.BuildServiceProvider().CreateScope();
    // var s1 = scope.ServiceProvider.GetRequiredService<IEventHandler<User, UserRegisterByEmail>>();
    //
    // var typeGeneric = typeof(IEventHandler<,>).MakeGenericType(typeof(User), typeof(UserRegisterByEmail));
    // var s2 = (IEventHandler)scope.ServiceProvider.GetRequiredService(typeGeneric);
}