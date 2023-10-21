using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Infrastructure.EntityConfigurations;

public class ClientRequestEntityTypeConfiguration : IEntityTypeConfiguration<ClientRequest>
{
    public void Configure(EntityTypeBuilder<ClientRequest> builder)
    {
        builder.ToTable("requests", OrderingDbContext.DEFAULT_SCHEMA);
        builder.HasKey(cr => cr.Id);
        builder.Property(cr => cr.Name).IsRequired();
        builder.Property(cr => cr.Time).IsRequired();
    }
    
    
}