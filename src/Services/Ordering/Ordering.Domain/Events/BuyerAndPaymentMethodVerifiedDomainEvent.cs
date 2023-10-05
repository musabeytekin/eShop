namespace Ordering.Domain.Events;

public class BuyerAndPaymentMethodVerifiedDomainEvent : INotification
{
    public Buyer Buyer { get; private set; }
    public PaymentMethod PaymentMethod { get; private set;}
    public int OrderId { get; private set; }
    
    public BuyerAndPaymentMethodVerifiedDomainEvent(Buyer buyer, PaymentMethod paymentMethod, int orderId)
    {
        Buyer = buyer;
        PaymentMethod = paymentMethod;
        OrderId = orderId;
    }
}