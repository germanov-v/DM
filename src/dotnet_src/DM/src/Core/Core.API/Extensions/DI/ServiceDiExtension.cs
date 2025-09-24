using System.Reflection;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Services;
using Core.Application.BusinessLogic.Identity;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Persistence.Seed;
using Core.Infrastructure.Services.Identity;

namespace Core.API.Extensions.DI;

public static class ServiceDiExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {

      

        var assemblies = new Assembly[] {
            typeof(CryptoIdentityService).Assembly,
            typeof(EmailPasswordUserProvider).Assembly,
          //  typeof(KafkaEventBus).Assembly,
            Assembly.GetExecutingAssembly()
        };
        //AssemblyHelper.GetAssembliesByMask(Assembly.GetExecutingAssembly(),  "Dashboard");

        var classes =
            assemblies
                .SelectMany(p =>
                    p.GetTypes().Where(type =>
                        type is
                        {
                            IsClass: true,
                            IsAbstract: false
                        } &&
                        typeof(IApplicationService).IsAssignableFrom(type)
                        
                    ));

        foreach (var item in classes)
        {
            var interfaces = item.GetInterfaces();
            var typeInterface = interfaces.FirstOrDefault(p =>
                p != typeof(IApplicationService)
            );
            if (typeInterface != null)
            {
                serviceCollection.AddScoped(typeInterface, item);
            }

        }


        serviceCollection.AddHttpContextAccessor();

     
        
        // serviceCollection.AddScoped<IdentityDbSeed>();
        // serviceCollection.AddScoped<BlogPostDbSeed>();
        // serviceCollection.AddScoped<ReferenceSourceService>();

        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
        serviceCollection.AddSingleton<TimeProvider>(TimeProvider.System);
        serviceCollection.AddTransient<IdentityDbSeed>();
        return serviceCollection;
    }
}