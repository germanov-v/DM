using Core.Infrastructure.Database;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Core.API.Extensions.Database;

public static class ConfigDatabase
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureConnection(IConfiguration configuration)
        {
            var dbOptions = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<CoreDbContext>(options =>
            {
                options.UseNpgsql(dbOptions
                        //        , b => b.MigrationsAssembly("Core.Infrastructure.Database")
                    )
                    //.EnableSensitiveDataLogging()
                    .UseSnakeCaseNamingConvention();
            });

            
            return services;
        }
    }
}