using System.Reflection;
using Core.Application.Abstractions;
using Core.Application.Services.Identity;
using Core.Application.SharedServices;
using Core.Domain.SharedKernel.Abstractions;

namespace Core.API.Extensions.DI;

public static class ServiceDiExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {

      

        var assemblies = new Assembly[] {
            typeof(CryptoService).Assembly,
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
        return serviceCollection;
    }
}