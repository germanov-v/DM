using Core.Infrastructure.Persistence.Seed;

namespace Core.API.Extensions;

public static class DbSeedExtension
{
    public static async Task<IApplicationBuilder> UseDbSeed(this WebApplication app,
        CancellationToken cancellationToken
    )
    {
        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetService<IdentityDbSeed>()!.SeedData(cancellationToken);
        
     
         // await scope.ServiceProvider.GetService<ReferenceSourceService>()!.GetRegions(cancellationToken);
         // await scope.ServiceProvider.GetService<ReferenceSourceService>()!.GetSpecializations(cancellationToken);
         // await scope.ServiceProvider.GetService<ReferenceSourceService>()!.GetProfessions(cancellationToken);
         // await scope.ServiceProvider.GetService<ReferenceSourceService>()!.GetIndustries(cancellationToken);

        return app;
    }
}