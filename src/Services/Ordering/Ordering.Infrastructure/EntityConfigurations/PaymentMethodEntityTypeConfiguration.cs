namespace Ordering.Infrastructure.EntityConfigurations;

public class PaymentMethodEntityTypeConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("paymentmethods", OrderingDbContext.DEFAULT_SCHEMA);
        builder.HasKey(p => p.Id);

        builder.Ignore(p => p.DomainEvents);

        builder.Property(p => p.Id)
            .UseHiLo("paymentseq", OrderingDbContext.DEFAULT_SCHEMA);

        builder.Property<int>("BuyerId")
            .IsRequired();

        builder
            .Property<string>("CardHolder")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("CardHolderName")
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property<string>("_cardNumber")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("CardNumber")
            .HasMaxLength(25)
            .IsRequired();

        builder
            .Property<DateTime>("_expiration")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Expiration")
            .HasMaxLength(25)
            .IsRequired();

        builder
            .Property<int>("_cardTypeId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("CardTypeId")
            .IsRequired();

        builder
            .HasOne(p => p.CardType)
            .WithMany()
            .HasForeignKey("_cardTypeId");
    }
}