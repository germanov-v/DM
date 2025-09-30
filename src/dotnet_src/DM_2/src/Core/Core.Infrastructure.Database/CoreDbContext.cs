using Core.Domain.BoundedContext.Identity.Aggregates;
using Core.Infrastructure.Database.Configurations.Identity;
using Core.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Database;

/// <summary>
/// https://learn.microsoft.com/en-us/ef/core/cli/dotnet
///  dotnet ef --project ../Core.Infrastructure.Database/ --startup-project ../Core.API/ migrations add Initial --context CoreDbContext --verbose
/// 
/// </summary>
public class CoreDbContext(DbContextOptions<CoreDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Session> Sessions => Set<Session>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ALARM");
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new SessionConfiguration());
        
        
        modelBuilder.SharedTypeEntity<Dictionary<string, object>>(
            "user_roles", j =>
            {
                j.ToTable("user_roles", SchemasDbConstants.Identity);

                j.Property<long>("user_id");
                j.Property<long>("role_id");
                j.HasKey("user_id", "role_id");

                j.HasOne<User>()
                    .WithMany()
                    .HasForeignKey("user_id")
                    .HasPrincipalKey(u => u.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                j.HasOne<Role>()
                    .WithMany()
                    .HasForeignKey("role_id")
                    .HasPrincipalKey(r => r.Id)
                    .OnDelete(DeleteBehavior.Restrict);
            });
    }


}