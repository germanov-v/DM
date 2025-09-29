using Core.Domain.BoundedContext.Identity.Aggregates;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Infrastructure.Migrations.Configurations.Abstractions;
using Core.Infrastructure.Migrations.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Infrastructure.Migrations.Configurations.Identity;

internal class SessionConfiguration: AggregateConfiguration<Session>
{
    public override void Configure(EntityTypeBuilder<Session> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("sessions", SchemasDbConstants.Identity);

        builder.ConfigureGuidId();
        

        // TODO:  объеденить в единую конфигурацию Provider
        builder.Property(e => e.Provider).HasMaxLength(10).IsRequired();
        builder.HasIndex(e => e.Provider);
        
        builder.Property(e => e.RefreshToken).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.RefreshToken)
            .IsUnique()
            .IncludeProperties(new[]{nameof(Session.Provider), nameof(Session.RefreshTokenExpiresAt)})
            ;


        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}