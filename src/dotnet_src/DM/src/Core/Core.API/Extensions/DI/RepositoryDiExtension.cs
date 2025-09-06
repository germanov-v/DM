using System.Reflection;
using Core.Application.Abstractions;
using Core.Application.Handlers.Identity;
using Core.Application.Options.Db;
using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.Entities;
using Core.Infrastructure.Persistence;
using Npgsql;

namespace Core.API.Extensions.DI;

public static class RepositoryDiExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
     
        var dbOptions = configuration.GetConnectionString("DefaultConnection");
        //configuration.GetSection("DefaultConnection").Bind(dbOptions);
        serviceCollection.AddOptions<DbConnectionOptions>()
            .Configure(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("DefaultConnection")
                                           ?? throw new InvalidOperationException("No connection string configured");
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
      
    
      
       
       var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
       NpgsqlLoggingConfiguration.InitializeLogging(loggerFactory);


       var assemblies = new Assembly []
       {
           typeof(UnitOfWork).Assembly,
           typeof(IdentityHandler).Assembly,
           typeof(IEntity).Assembly,
       };
      

        var classes =
            assemblies.SelectMany(p=>p.GetTypes().Where(type =>
                type is
                {
                    IsClass: true,
                    IsAbstract: false
                } &&
                typeof(IRepository).IsAssignableFrom(type)
            ));
        //Assembly.GetExecutingAssembly().GetTypes();


        var baseRepositoriesInterfaces = new string[] { "IQueryRepository", "ICommandRepository" };
        foreach (var item in classes)
        {
            var interfaces = item.GetInterfaces();
            var typeInterface = interfaces.FirstOrDefault(p =>
                    !p.IsGenericType
                    && p.Name.Contains("Repository")
                    //  (p.Name.Contains("CommandRepository") || p.Name.Contains("QueryRepository"))
                    && !baseRepositoriesInterfaces.Contains(p.Name)
                //  !p.Name.Contains("CommandRepository")&& 
                //  !p.Name.Contains("QueryRepository")
            );
            if (typeInterface != null)
            {
                serviceCollection.AddScoped(typeInterface, item);
            }
        }


        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
        serviceCollection.AddScoped<IConnectionFactory<NpgsqlConnection>, PgsqlConnectionFactory>();
        // serviceCollection.AddScoped<INotificationTracker, DomainEventTracker>();
        // serviceCollection.AddScoped<IBaseDomainEventTracker, DomainEventTracker>();
      
        return serviceCollection;
    }
    
    
    // public static async Task<IApplicationBuilder> UseRepositoriesConfig(this WebApplication app
    // )
    // {
    //     await using var scope = app.Services.CreateAsyncScope();
    //     var services = scope.ServiceProvider;
    //     var dbContext = services.GetRequiredService<CoreDbContext>();
    //     DapperMapperEfCore.Map([]);
    //     return app;
    // }
}