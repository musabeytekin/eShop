namespace Ordering.Infrastructure.EntityConfigurations;

public class OrderStatusEntityTypeConfiguration: IEntityTypeConfiguration<OrderStatus>
{
    public void Configure(EntityTypeBuilder<OrderStatus> builder)
    {
        builder.ToTable("orderstatus", OrderingDbContext.DEFAULT_SCHEMA);

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasDefaultValue(1)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(o => o.Name)
            .HasMaxLength(200)
            .IsRequired();
    }
}