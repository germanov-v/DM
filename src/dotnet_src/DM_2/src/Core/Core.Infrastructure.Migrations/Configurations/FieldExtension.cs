using Core.Domain.SharedKernel.Entities.Abstractions.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Infrastructure.Migrations.Configurations;

public static class FieldExtension
{
    extension<T>(EntityTypeBuilder<T> builder)
        where T : class, IGuidId
    {
        public void ConfigureGuidId()
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.GuidId).IsRequired();
            builder.HasIndex(x => x.GuidId);
        }
    }
    
    // TODO: дубль убрать через ConfigureGuidIdCore - и Expression
    extension<TEntity, TChild>(OwnedNavigationBuilder<TEntity, TChild> builder)
        where TEntity : class
        where TChild: class, IGuidId
    {
        public void ConfigureGuidId()
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.GuidId).IsRequired();
            builder.HasIndex(x => x.GuidId);
        }
    }


    extension<TEntity, TChild>(OwnedNavigationBuilder<TEntity, TChild> builder)
        where TEntity : class
        where TChild: class, IConfirmed
    {
        
       
        public void ConfigureConfirmed()
        {
            var confirmedCodeExpiresNameField = "confirmed_code_expires_at";
            var isConfirmed = "is_confirmed";
            builder.Property(x => x.IsConfirmed)
                .HasColumnName(isConfirmed)
                .IsRequired();
            builder.Property(x => x.ConfirmedAt);
            builder.Property(x => x.ConfirmedCodeExpiresAt)
                .HasColumnName(confirmedCodeExpiresNameField);
            builder.HasIndex(x => x.ConfirmedCode)
               // .HasDatabaseName("idx_confirmed_code")
                .IncludeProperties(x=>  x.ConfirmedCodeExpiresAt!)
                .IncludeProperties(x=>  x.ConfirmedAt!);
               ;
            
        }
    }
    
    
}