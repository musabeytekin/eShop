namespace Ordering.Infrastructure.EntityConfigurations;

public class CardTypeEntityTypeConfiguration : IEntityTypeConfiguration<CardType>
{
    public void Configure(EntityTypeBuilder<CardType> builder)
    {

        builder.ToTable("cardtypes", OrderingDbContext.DEFAULT_SCHEMA);
        
        builder.HasKey(ct => ct.Id);
        
        builder.Property(ct => ct.Id)
            .HasDefaultValue(1)
            .ValueGeneratedNever()
            .IsRequired();
        
        builder.Property(ct => ct.Name)
            .HasMaxLength(200)
            .IsRequired();
    }
}