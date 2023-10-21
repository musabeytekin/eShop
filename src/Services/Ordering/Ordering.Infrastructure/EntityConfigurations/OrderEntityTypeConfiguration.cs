namespace Ordering.Infrastructure.EntityConfigurations;

public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", OrderingDbContext.DEFAULT_SCHEMA);

        builder.HasKey(o => o.Id);

        builder.Ignore(o => o.DomainEvents);

        builder.Property(o => o.Id)
            .UseHiLo("orderseq", OrderingDbContext.DEFAULT_SCHEMA);

        builder.OwnsOne(o => o.Address, a =>
        {
            a.Property<int>("OrderId").UseHiLo("orderseq", OrderingDbContext.DEFAULT_SCHEMA);

            a.WithOwner();
        });

        builder.Property<int?>("_buyerId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("BuyerId")
            .IsRequired(false);

        builder
            .Property<DateTime>("_orderDate")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("OrderDate")
            .IsRequired();

        builder
            .Property<int>("_orderStatusId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("OrderStatusId")
            .IsRequired();

        builder
            .Property<int?>("_paymentMethodId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("PaymentMethodId")
            .IsRequired(false);

        builder.Property<string>("Description").IsRequired(false);

        var navigation = builder.Metadata.FindNavigation(nameof(Order.OrderItems));
        
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasOne<PaymentMethod>()
            .WithMany()
            .HasForeignKey("_paymentMethodId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<Buyer>()
            .WithMany()
            .IsRequired(false)
            .HasForeignKey("_buyerId");

        builder.HasOne(o => o.OrderStatus)
            .WithMany()
            .HasForeignKey("_orderStatusId");
    }
}