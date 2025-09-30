using Core.Domain.BoundedContext.Identity.Aggregates;
using Core.Infrastructure.Database.Configurations.Abstractions;
using Core.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Infrastructure.Database.Configurations.Identity;

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