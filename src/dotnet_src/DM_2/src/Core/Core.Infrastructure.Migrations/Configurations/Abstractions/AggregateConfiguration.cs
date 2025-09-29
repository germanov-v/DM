using Core.Domain.SharedKernel.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Infrastructure.Migrations.Configurations.Abstractions;

internal abstract class AggregateConfiguration<TAggregate> : IEntityTypeConfiguration<TAggregate> where TAggregate
     :  AggregateRoot
{
     public virtual void Configure(EntityTypeBuilder<TAggregate> builder)
     {
        
     }
}