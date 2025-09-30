using Core.Domain.BoundedContext.Identity.Aggregates;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Infrastructure.Database.Configurations.Abstractions;
using Core.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Infrastructure.Database.Configurations.Identity;

internal class UserConfiguration : AggregateConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("users", SchemasDbConstants.Identity);

        builder.ConfigureGuidId();
        

        builder.Property(x => x.Name).HasMaxLength(1024).IsRequired();
        builder.HasIndex(x => x.Name);
        
        builder.Property(x => x.IsConfirmed).IsRequired();
        builder.Property(x => x.ConfirmedChangeAt).IsRequired();
        
        
        builder.Property(x => x.IsBlocked).IsRequired();
        builder.Property(x => x.BlockedChangedAt).IsRequired();
        
  
        builder.OwnsOne(x=>x.Email, email =>
        {
            email.ToTable("users_email", SchemasDbConstants.Identity);
            email.WithOwner().HasForeignKey("user_id");
            email.HasKey("user_id");
            
            email.Property(e => e.Email).HasMaxLength(100).IsRequired();
            email.HasIndex(e=>e.Email);

            email.ConfigureConfirmed();
        });
        
        builder.OwnsOne(x=>x.Phone, phone =>
        {
            phone.ToTable("users_phone", SchemasDbConstants.Identity);
            phone.WithOwner().HasForeignKey("user_id");
            phone.HasKey("user_id");
            
            phone.Property(e => e.Phone).HasMaxLength(100).IsRequired();
            phone.HasIndex(e=>e.Phone);

            phone.ConfigureConfirmed();
        });
        
        builder.OwnsMany(x => x.ExternalIdentities, ext =>
            {
                ext.ToTable("external_providers",SchemasDbConstants.Identity);
                ext.WithOwner().HasForeignKey("user_id");

                ext.ConfigureGuidId(); 

                ext.Property(e => e.Provider).HasMaxLength(10).IsRequired();
                ext.HasIndex(e => e.Provider);
                ext.Property(e => e.ProviderUserId).IsRequired();
                ext.HasIndex(e => e.ProviderUserId);

           
                ext.Property(e => e.RawProfileJson).HasColumnType("jsonb");
                ext.Property(e => e.IsPrimary).IsRequired();

                // ext.Property(e => e.CreatedAt);
                // ext.Property(e => e.UpdatedAt);
                // ext.Property(e => e.LastUsedAt);

                 ext.HasIndex(e => new { e.Provider, e.ProviderUserId })
                 //  .HasDatabaseName("ux_external_identity_provider_user")
                   .IsUnique();

            
                ext.HasIndex("user_id", nameof(ExternalIdentity.Provider))
                   .IsUnique()
                  // .HasFilter("is_primary")
                   ;
            });

        // работаем напрямую с приватным полем
        // builder.Metadata.FindNavigation(nameof(User.RoleIds))!
        //     .SetPropertyAccessMode(PropertyAccessMode.Field);
        
        // builder.ModelBuilder.SharedTypeEntity<Dictionary<string, object>>(
        //     name: "user_roles", configure: j =>
        //     {
        //         j.ToTable("user_roles", SchemasDbConstants.Identity);
        //
        //         j.Property<long>("user_id");
        //         j.Property<long>("role_id");
        //         j.HasKey("user_id", "role_id");
        //
        //         j.HasOne<User>()
        //             .WithMany()                     // ← нет навигации из User
        //             .HasForeignKey("user_id")
        //             .HasPrincipalKey(u => u.Id)
        //             .OnDelete(DeleteBehavior.Cascade);
        //
        //         j.HasOne<Role>()
        //             .WithMany()                     // ← нет навигации из Role
        //             .HasForeignKey("role_id")
        //             .HasPrincipalKey(r => r.Id)
        //             .OnDelete(DeleteBehavior.Restrict);
        //     });
        
        // builder.HasMany<Role>()
        //     .WithMany()
        //     .UsingEntity("user_roles", //p.ToTable("user_roles"),
        //         l => l.HasOne(typeof(User)).WithMany().HasForeignKey("user_id").HasPrincipalKey(nameof(User.Id)),
        //         r => r.HasOne(typeof(Role)).WithMany().HasForeignKey("role_id").HasPrincipalKey(nameof(Role.Id)),
        //         j =>
        //         {
        //             j.ToTable("user_roles", SchemasDbConstants.Identity);
        //             j.HasKey("user_id", "role_id");
        //         });

    }
}