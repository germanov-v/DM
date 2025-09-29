using Core.Domain.BoundedContext.Identity.Aggregates;
using Core.Infrastructure.Migrations.Configurations.Abstractions;
using Core.Infrastructure.Migrations.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Infrastructure.Migrations.Configurations.Identity;

internal class RoleConfiguration: AggregateConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.ToTable("roles", SchemasDbConstants.Identity);

        builder.ConfigureGuidId();
        
        builder.Property(x => x.Name).HasMaxLength(250);
        builder.Property(x=>x.Alias).HasMaxLength(50);

    }
}